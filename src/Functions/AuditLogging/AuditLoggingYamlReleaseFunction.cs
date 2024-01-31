#nullable enable

using Microsoft.Azure.WebJobs;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.Approvals.Services;
using Rabobank.Compliancy.Core.PipelineResources.Services;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Domain.Rules;
using Rabobank.Compliancy.Functions.AuditLogging.Helpers;
using Rabobank.Compliancy.Functions.AuditLogging.Model;
using Rabobank.Compliancy.Functions.AuditLogging.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.StorageClient;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Response = Rabobank.Compliancy.Infra.AzdoClient.Response;

namespace Rabobank.Compliancy.Functions.AuditLogging;

public class AuditLoggingYamlReleaseFunction
{
    private readonly IAzdoRestClient _azdoClient;
    private readonly ILoggingService _loggingService;
    private readonly IYamlReleaseDeploymentEventParser _eventParser;
    private readonly IPipelineRegistrationRepository _registrationRepository;
    private readonly IYamlReleaseApproverService _pipelineApproverService;
    private readonly IPullRequestApproverService _pullRequestApproverService;
    private readonly IBuildPipelineService _buildPipelineService;
    private readonly IRepositoryService _repoService;
    private readonly IEnumerable<IBuildPipelineRule> _buildPipelineRules;
    private readonly IMonitorDecoratorService _monitorDecorator;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Legacy. Will be refactored completely")]
    public AuditLoggingYamlReleaseFunction(
        IAzdoRestClient azdoClient,
        ILoggingService loggingService,
        IYamlReleaseDeploymentEventParser eventParser,
        IPipelineRegistrationRepository registrationRepository,
        IYamlReleaseApproverService pipelineApproverService,
        IPullRequestApproverService pullRequestApproverService,
        IBuildPipelineService buildPipelineService,
        IRepositoryService repoService,
        IEnumerable<IBuildPipelineRule> buildPipelineRules,
        IMonitorDecoratorService monitorDecoratorService)
    {
        _azdoClient = azdoClient;
        _loggingService = loggingService;
        _eventParser = eventParser;
        _registrationRepository = registrationRepository;
        _pipelineApproverService = pipelineApproverService;
        _pullRequestApproverService = pullRequestApproverService;
        _buildPipelineService = buildPipelineService;
        _repoService = repoService;
        _buildPipelineRules = buildPipelineRules;
        _monitorDecorator = monitorDecoratorService;
    }

    [FunctionName(nameof(AuditLoggingYamlReleaseFunction))]
    public async Task RunAsync([QueueTrigger(StorageQueueNames.AuditYamlReleaseQueueName,
        Connection = "eventQueueStorageConnectionString")] string data)
    {
        var evt = default(YamlReleaseDeploymentEvent);

        try
        {
            evt = _eventParser.Parse(data);

            await _monitorDecorator.MonitorDecoratorYamlReleaseAsync(evt.Organization, evt.ProjectId, evt.RunId, evt.StageName);

            if (!evt.IsDeploymentExecuted())
            {
                return;
            }

            var pipelineRegistrations = await _registrationRepository.GetAsync(
                evt.Organization, evt.ProjectId, evt.PipelineId, evt.StageName);
            if (!pipelineRegistrations.Any())
            {
                return;
            }

            var project = await _azdoClient.GetAsync(Project.ProjectById(
                evt.ProjectId), evt.Organization) ?? throw new InvalidOperationException($"Unable to find project: {evt.ProjectId}");
            var pipelineRun = await _azdoClient.GetAsync(Builds.Build(
                evt.ProjectId, evt.RunId), evt.Organization) ?? throw new InvalidOperationException($"Unable to find pipelineRun: {evt.RunId}");
            var tag = pipelineRun.Tags.LastOrDefault(t => t.IsChangeTag());

            var pipelineApproval = await _pipelineApproverService.HasApprovalAsync(
                project, evt.RunId, pipelineRun.RequestedBy.UniqueName, evt.Organization);
            var pullRequestApproval = await _pullRequestApproverService.HasApprovalAsync(
                evt.ProjectId, evt.RunId, evt.Organization);

            var pipeline = await _azdoClient.GetAsync(Builds.BuildDefinition(
                evt.ProjectId, evt.PipelineId), evt.Organization);

            var buildPipelines = await _buildPipelineService.GetLinkedPipelinesAsync(
                evt.Organization, pipeline);
            var buildUrls = buildPipelines
                .Select(b => b.Links.Web.Href)
                .ToList();

            var repositories = await _buildPipelineService.GetLinkedRepositoriesAsync(evt.Organization,
                buildPipelines.Concat(new List<Response.BuildDefinition> { pipeline }).ToList());
            var repoUrls = (await Task.WhenAll(repositories.Select(async x =>
                await _repoService.GetUrlAsync(evt.Organization, project, x)))).ToList();

            var rule = _buildPipelineRules.First(r => r.Name == RuleNames.BuildPipelineHasSonarqubeTask);
            var fortifyRule = _buildPipelineRules.First(r => r.Name == RuleNames.BuildPipelineHasFortifyTask);
            bool fortifyRan;
            bool sonarRan;

            if (buildPipelines.Any())
            {
                sonarRan = (await Task.WhenAll(buildPipelines.Select(async d => await rule.EvaluateAsync(
                    evt.Organization, d.Project.Id, d)))).All(a => a);

                fortifyRan = (await Task.WhenAll(buildPipelines.Select(async d => await fortifyRule.EvaluateAsync(
                    evt.Organization, d.Project.Id, d)))).All(a => a);
            }
            else
            {
                sonarRan = await rule.EvaluateAsync(evt.Organization, evt.ProjectId, pipeline);
                fortifyRan = await fortifyRule.EvaluateAsync(evt.Organization, evt.ProjectId, pipeline);
            }

            var report = CreateReport(evt, pipelineRegistrations, project, pipelineRun, tag,
                pipelineApproval, pullRequestApproval, buildUrls, repoUrls, sonarRan, fortifyRan);
            await _loggingService.LogInformationAsync(LogDestinations.AuditDeploymentLog, report);
        }
        catch (Exception e)
        {
            var exceptionBaseMetaInformation = new ExceptionBaseMetaInformation
                (nameof(AuditLoggingYamlReleaseFunction), evt?.Organization, evt?.ProjectId)
            {
                RunUrl = evt?.RunUrl,
                RequestData = data
            };
            await _loggingService.LogExceptionAsync(LogDestinations.AuditLoggingErrorLog, exceptionBaseMetaInformation, e);
            throw;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Legacy. Will be refactored completely")]
    private static AuditLoggingReport CreateReport(YamlReleaseDeploymentEvent evt,
        IEnumerable<PipelineRegistration> pipelineRegistrations, Response.Project project,
        Response.Build pipelineRun, string? tag, bool pipelineApproval, bool pullRequestApproval,
        IEnumerable<Uri> buildUrls, IEnumerable<Uri> repoUrls, bool sonarRan, bool fortifyRan) =>
        new()
        {
            Organization = evt.Organization,
            ProjectName = project.Name,
            ProjectId = evt.ProjectId,
            PipelineName = evt.PipelineName,
            PipelineId = evt.PipelineId,
            StageName = evt.StageName,
            StageId = evt.StageId,
            RunName = evt.RunName,
            RunId = evt.RunId,
            RunUrl = evt.RunUrl,
            DeploymentStatus = evt.DeploymentStatus,
            CreatedDate = evt.CreatedDate,
            CompletedOn = pipelineRun.StartTime,
            PipelineApproval = pipelineApproval,
            PullRequestApproval = pullRequestApproval,
            ArtifactIntegrity = true,
            Sm9ChangeId = tag.ChangeId(),
            Sm9ChangeUrl = tag.ChangeUrl(),
            IsSox = pipelineRegistrations.IsSox(),
            CiIdentifier = pipelineRegistrations.CiIdentifiers(),
            CiName = pipelineRegistrations.CiNames(),
            BuildUrls = buildUrls,
            RepoUrls = repoUrls,
            SonarRan = sonarRan,
            FortifyRan = fortifyRan
        };
}