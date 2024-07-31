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
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.StorageClient;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Environment = Rabobank.Compliancy.Infra.AzdoClient.Response.Environment;
using Requests = Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.AuditLogging;

public class AuditLoggingClassicReleaseFunction
{
    private const string _artifactTypeBuild = "build";

    private static readonly string[] _unExecutedDeploymentStatuses =
        { "canceled", "skipped", "notDeployed" };

    private readonly IAzdoRestClient _azdoClient;
    private readonly ILoggingService _loggingService;
    private readonly IClassicReleaseDeploymentEventParser _eventParser;
    private readonly IPipelineRegistrationRepository _registrationRepository;
    private readonly IClassicReleaseApproverService _pipelineApproverService;
    private readonly IPullRequestApproverService _pullRequestApproverService;
    private readonly IReleasePipelineService _releasePipelineService;
    private readonly IRepositoryService _repoService;
    private readonly IEnumerable<IBuildPipelineRule> _buildPipelineRules;
    private readonly IMonitorDecoratorService _monitorDecoratorService;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Legacy. Will be refactored completely")]
    public AuditLoggingClassicReleaseFunction(
        IAzdoRestClient azdoClient,
        ILoggingService loggingService,
        IClassicReleaseDeploymentEventParser eventParser,
        IPipelineRegistrationRepository registrationRepository,
        IClassicReleaseApproverService pipelineApproverService,
        IPullRequestApproverService pullRequestApproverService,
        IReleasePipelineService releasePipelineResourcesService,
        IRepositoryService repoService,
        IEnumerable<IBuildPipelineRule> buildPipelineRules,
        IMonitorDecoratorService monitorDecorator)
    {
        _azdoClient = azdoClient;
        _loggingService = loggingService;
        _eventParser = eventParser;
        _registrationRepository = registrationRepository;
        _pipelineApproverService = pipelineApproverService;
        _pullRequestApproverService = pullRequestApproverService;
        _releasePipelineService = releasePipelineResourcesService;
        _repoService = repoService;
        _buildPipelineRules = buildPipelineRules;
        _monitorDecoratorService = monitorDecorator;
    }

    [FunctionName(nameof(AuditLoggingClassicReleaseFunction))]
    public async Task RunAsync(
        [QueueTrigger(StorageQueueNames.AuditClassicReleaseQueueName,
            Connection = "eventQueueStorageConnectionString")]
        string data)
    {
        var deploymentEvent = default(ClassicReleaseDeploymentEvent);

        try
        {
            deploymentEvent = _eventParser.Parse(data);
            if (deploymentEvent == null)
            {
                return;
            }

            var release = await _azdoClient.GetAsync(Requests.ReleaseManagement.Release(
                deploymentEvent.ProjectId, deploymentEvent.ReleaseId), deploymentEvent.Organization);
            if (release == null)
            {
                return;
            }

            await _monitorDecoratorService.MonitorDecoratorClassicReleaseAsync(deploymentEvent.Organization, deploymentEvent.ProjectId, release, deploymentEvent.StageName);

            var environment = release.Environments?.FirstOrDefault(e => e.Name == deploymentEvent.StageName);
            if (environment == null || _unExecutedDeploymentStatuses.Contains(environment.Status))
            {
                return;
            }

            var pipelineRegistrations = await _registrationRepository.GetAsync(
                deploymentEvent.Organization, deploymentEvent.ProjectId, release.ReleaseDefinition?.Id,
                environment.DefinitionEnvironmentId);
            if (!pipelineRegistrations.Any())
            {
                return;
            }

            var tag = release.Tags?.LastOrDefault(t => t.IsChangeTag());

            var pipelineApproval = await _pipelineApproverService.HasApprovalAsync(
                deploymentEvent.ProjectId, deploymentEvent.ReleaseId, release.CreatedBy?.Id.ToString(), deploymentEvent.Organization);

            var buildAndProjectIds = GetBuildIds(release);
            var pullRequestApproval = buildAndProjectIds != null && buildAndProjectIds.Any() &&
                                      (await Task.WhenAll(buildAndProjectIds
                                          .Select(b => _pullRequestApproverService.HasApprovalAsync(
                                              b.projectId, b.buildId, deploymentEvent.Organization))))
                                      .All(a => a);

            var artifactTypes = release.Artifacts?.Select(a => a.Type).ToList();
            var artifactIntegrity = artifactTypes != null && artifactTypes.Any() &&
                                    artifactTypes.All(a => a.Equals(_artifactTypeBuild, StringComparison.InvariantCultureIgnoreCase));

            var project = await _azdoClient.GetAsync(Requests.Project.ProjectById(
                deploymentEvent.ProjectId), deploymentEvent.Organization);
            var releasePipeline = await _azdoClient.GetAsync(Requests.ReleaseManagement.Definition(
                deploymentEvent.ProjectId, release.ReleaseDefinition?.Id), deploymentEvent.Organization);

            var buildPipelines = (await _releasePipelineService.GetLinkedPipelinesAsync(
                deploymentEvent.Organization, releasePipeline, deploymentEvent.ProjectId)).ToList();
            var buildUrls = buildPipelines
                .Select(b => b.Links.Web.Href)
                .ToList();

            var repositories = await _releasePipelineService.GetLinkedRepositoriesAsync(
                deploymentEvent.Organization, new List<ReleaseDefinition> { releasePipeline }, buildPipelines);
            var repoUrls = (await Task.WhenAll(repositories.Select(async x =>
                await _repoService.GetUrlAsync(deploymentEvent.Organization, project, x)))).Distinct().ToList();

            var rule = _buildPipelineRules.First(r => r.Name == RuleNames.BuildPipelineHasSonarqubeTask);
            var sonarRan = buildPipelines.Any() && (await Task.WhenAll(buildPipelines
                .Select(async d => await rule.EvaluateAsync(deploymentEvent.Organization, d.Project.Id, d))))
                .All(a => a);

            var ruleFortify = _buildPipelineRules.First(r => r.Name == RuleNames.BuildPipelineHasFortifyTask);
            var fortifyRan = buildPipelines.Any() && (await Task.WhenAll(buildPipelines
                .Select(async d => await ruleFortify.EvaluateAsync(deploymentEvent.Organization, d.Project.Id, d))))
                .All(a => a);

            var report = CreateReport(deploymentEvent, release, environment, pipelineRegistrations, tag,
                pipelineApproval, pullRequestApproval, artifactIntegrity, buildUrls, repoUrls, sonarRan, fortifyRan);

            await _loggingService.LogInformationAsync(LogDestinations.AuditDeploymentLog, report);
        }
        catch (Exception e)
        {
            var exceptionBaseMetaInformation = new ExceptionBaseMetaInformation
                (nameof(AuditLoggingClassicReleaseFunction), deploymentEvent?.Organization, deploymentEvent?.ProjectId)
            {
                ReleaseUrl = deploymentEvent?.ReleaseUrl,
                RequestData = data
            };
            await _loggingService.LogExceptionAsync(LogDestinations.AuditLoggingErrorLog, exceptionBaseMetaInformation, e);
            throw;
        }
    }

    private static IEnumerable<(string? buildId, string? projectId)>? GetBuildIds(Release release) =>
        release.Artifacts?
            .Where(t => t.Type.Equals(_artifactTypeBuild, StringComparison.InvariantCultureIgnoreCase))
            .Select(a => (buildId: a.DefinitionReference?.Version?.Id, projectId: a.DefinitionReference?.Project?.Id));

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Legacy. Will be refactored completely")]
    private static AuditLoggingReport CreateReport(ClassicReleaseDeploymentEvent evt, Release release,
        Environment environment, IEnumerable<PipelineRegistration> pipelineRegistrations, string tag,
        bool pipelineApproval, bool pullRequestApproval, bool artifactIntegrity,
        IEnumerable<Uri> buildUrls, IEnumerable<Uri> repoUrls, bool sonarRan, bool fortifyRan) =>
        new()
        {
            Organization = evt.Organization,
            ProjectName = evt.ProjectName,
            ProjectId = evt.ProjectId,
            PipelineName = release.ReleaseDefinition?.Name,
            PipelineId = release.ReleaseDefinition?.Id,
            StageName = evt.StageName,
            StageId = environment.DefinitionEnvironmentId,
            RunName = release.Name,
            RunId = evt.ReleaseId,
            RunUrl = evt.ReleaseUrl,
            DeploymentStatus = environment.Status,
            CreatedDate = evt.CreatedDate,
            CompletedOn = environment.CreatedOn,
            PipelineApproval = pipelineApproval,
            PullRequestApproval = pullRequestApproval,
            ArtifactIntegrity = artifactIntegrity,
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