#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Flurl.Http;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Core.PipelineResources.Services;
using Rabobank.Compliancy.Core.Rules.Extensions;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.RuleProfiles;
using Rabobank.Compliancy.Domain.Rules;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using Rabobank.Compliancy.Functions.PipelineBreaker.Model;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Extensions;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using Rabobank.Compliancy.Infrastructure.Constants;
using Rabobank.Compliancy.Infrastructure.Dto.CompliancyReport;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using ErrorMessages = Rabobank.Compliancy.Functions.PipelineBreaker.Exceptions.ErrorMessages;
using Project = Rabobank.Compliancy.Infra.AzdoClient.Response.Project;
using Repository = Rabobank.Compliancy.Infra.AzdoClient.Response.Repository;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.PipelineBreaker.Services;

public class PipelineBreakerService : IPipelineBreakerService
{
    private const string _stagesSelector = "stages[*].stage";
    private const string _defaultStageName = "__default";

    private readonly IAzdoRestClient _azdoClient;
    private readonly IEnumerable<IBuildPipelineRule> _buildPipelineRules;
    private readonly IBuildPipelineService _buildPipelineService;
    private readonly IEnumerable<IClassicReleasePipelineRule> _classicReleasePipelineRules;
    private readonly ComplianceConfig _config;
    private readonly IDeviationStorageRepository _deviationRepo;
    private readonly IExtensionDataRepository _extensionDataRepository;
    private readonly ILogQueryService _logQueryService;
    private readonly IEnumerable<IProjectRule> _projectRules;
    private readonly IReleasePipelineService _releasePipelineService;
    private readonly IRepositoryService _repoService;
    private readonly IEnumerable<IRepositoryRule> _repositoryRules;
    private readonly IEnumerable<IYamlReleasePipelineRule> _yamlReleasePipelineRules;

    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters",
        Justification = "Legacy. Will be refactored completely")]
    public PipelineBreakerService(
        IAzdoRestClient azdoClient,
        ILogQueryService logQueryService,
        ComplianceConfig config,
        IEnumerable<IProjectRule> projectRules,
        IEnumerable<IClassicReleasePipelineRule> classicReleasePipelineRules,
        IEnumerable<IYamlReleasePipelineRule> yamlReleasePipelineRules,
        IEnumerable<IBuildPipelineRule> buildPipelineRules,
        IEnumerable<IRepositoryRule> repositoryRules,
        IDeviationStorageRepository deviationRepo,
        IBuildPipelineService buildPipelineService,
        IReleasePipelineService releasePipelineService,
        IRepositoryService repoService,
        IExtensionDataRepository extensionDataRepository)
    {
        _azdoClient = azdoClient;
        _logQueryService = logQueryService;
        _config = config;
        _projectRules = projectRules;
        _classicReleasePipelineRules = classicReleasePipelineRules;
        _yamlReleasePipelineRules = yamlReleasePipelineRules;
        _buildPipelineRules = buildPipelineRules;
        _repositoryRules = repositoryRules;
        _deviationRepo = deviationRepo;
        _buildPipelineService = buildPipelineService;
        _releasePipelineService = releasePipelineService;
        _repoService = repoService;
        _extensionDataRepository = extensionDataRepository;
    }

    public async Task<PipelineBreakerRegistrationReport?> GetPreviousRegistrationResultAsync(PipelineRunInfo runInfo)
    {
        var query = $"{LogNames.RegistrationLogName}_CL" +
                    $"| where Organization_s == \"{runInfo.Organization}\" and " +
                    $"ProjectId_g == \"{runInfo.ProjectId}\" and RunId_s == \"{runInfo.RunId}\"" +
                    $"| order by TimeGenerated desc" +
                    $"| limit 1" +
                    $"| project RegistrationStatus_s, Result_d";

        var result = await _logQueryService.GetQueryEntryAsync<PipelineBreakerRegistrationReport>(query);
        return result ?? new PipelineBreakerRegistrationReport { Result = Domain.Enums.PipelineBreakerResult.None };
    }

    public async Task<PipelineBreakerReport?> GetPreviousComplianceResultAsync(PipelineRunInfo runInfo)
    {
        var query = $"{LogNames.ComplianceLogName}_CL" +
                    $"| where Organization_s == \"{runInfo.Organization}\" and " +
                    $"ProjectId_g == \"{runInfo.ProjectId}\" and RunId_s == \"{runInfo.RunId}\"" +
                    $"| order by TimeGenerated desc" +
                    $"| limit 1" +
                    $"| project RuleCompliancyReports_s, IsExcluded_b = coalesce(IsExcluded_b, false), Result_d";

        var result = await _logQueryService.GetQueryEntryAsync<PipelineBreakerReport>(query);
        return result ?? new PipelineBreakerReport { Result = Domain.Enums.PipelineBreakerResult.None };
    }

    public async Task<PipelineRunInfo> EnrichPipelineInfoAsync(PipelineRunInfo runInfo)
    {
        if (runInfo.PipelineType != null &&
            runInfo.PipelineType.Equals(ItemTypes.ReleasePipeline, StringComparison.InvariantCultureIgnoreCase))
        {
            return await EnrichWithReleaseInfoAsync(runInfo);
        }

        return await EnrichWithBuildInfoAsync(runInfo);
    }

    public async Task<IEnumerable<RuleCompliancyReport>> GetCompliancy(PipelineRunInfo runInfo,
        IEnumerable<PipelineRegistration> registrations)
    {
        var prodStages = GetProdStages(runInfo, registrations);

        // Get the profile from any of the registrations, if there are no registrations, or they have no profiles, instantiate the default profile.
        var profile = registrations.FirstOrDefault()?.GetRuleProfile() ?? new DefaultRuleProfile();

        if (!runInfo.IsProdStageRun(prodStages))
        {
            return new List<RuleCompliancyReport>();
        }

        var ruleReports = new List<RuleCompliancyReport>();
        var projectReport = await GetCompliancyForProjectPermissionRules(runInfo, profile);

        ruleReports.AddRange(projectReport);

        if (runInfo.IsClassicPipeline)
        {
            var buildPipelines = await _releasePipelineService.GetLinkedPipelinesAsync(
                runInfo.Organization, runInfo.ClassicReleasePipeline, runInfo.ProjectId);

            var repositories = await _releasePipelineService.GetLinkedRepositoriesAsync(
                runInfo.Organization, new List<ReleaseDefinition?> { runInfo.ClassicReleasePipeline }, buildPipelines);

            var classicPipelinesReport =
                await GetCompliancyForClassicReleasePipelineRules(runInfo, registrations, profile);
            var buildPipelinesReport =
                await GetCompliancyForBuildPipelineRules(runInfo, registrations, buildPipelines, profile);
            var repositoriesReport =
                await GetCompliancyForRepositoryRules(runInfo, registrations, repositories, profile);

            ruleReports.AddRange(classicPipelinesReport);
            ruleReports.AddRange(buildPipelinesReport);
            ruleReports.AddRange(repositoriesReport);
        }
        else
        {
            var buildPipelines = await _buildPipelineService.GetLinkedPipelinesAsync(
                runInfo.Organization, runInfo.BuildPipeline);

            if (!buildPipelines.Any())
            {
                buildPipelines = new List<BuildDefinition?>
                    { runInfo.BuildPipeline }; // YamlRelease is used for CI & CD
            }

            var repositories = await _buildPipelineService.GetLinkedRepositoriesAsync(runInfo.Organization,
                buildPipelines.Concat(new List<BuildDefinition?> { runInfo.BuildPipeline }));

            var yamlPipelinesReport = await GetCompliancyForYamlReleasePipelineRules(runInfo, registrations, profile);
            var buildPipelinesReport =
                await GetCompliancyForBuildPipelineRules(runInfo, registrations, buildPipelines, profile);
            var repositoriesReport =
                await GetCompliancyForRepositoryRules(runInfo, registrations, repositories, profile);

            ruleReports.AddRange(yamlPipelinesReport);
            ruleReports.AddRange(buildPipelinesReport);
            ruleReports.AddRange(repositoriesReport);
        }

        //The GroupBy and Select are required, because YAML CiCd pipelines are scanned twice.
        //The NobodyCanDeleteBuilds rule is executed for both build pipelines and YAML release pipelines categories.
        return ruleReports
            .GroupBy(r => new { r.RuleDescription, r.ItemName })
            .Select(g => g.Last());
    }

    private async Task<PipelineRunInfo> EnrichWithReleaseInfoAsync(PipelineRunInfo runInfo)
    {
        var project = await _azdoClient.GetAsync(Infra.AzdoClient.Requests.Project.ProjectById(
            runInfo.ProjectId), runInfo.Organization);

        var release = await _azdoClient.GetAsync(ReleaseManagement.Release(
            runInfo.ProjectId, runInfo.RunId), runInfo.Organization);

        if (release == null || release.ReleaseDefinition == null)
        {
            throw new InvalidOperationException(ErrorMessages.ReleaseNotAvailableErrorMessage(runInfo.RunId));
        }

        var classicReleasePipeline = await _azdoClient.GetAsync(ReleaseManagement.Definition(
                runInfo.ProjectId, release.ReleaseDefinition.Id, release.ReleaseDefinitionRevision),
            runInfo.Organization);

        // Sometimes it happens that a revision number is returned from the release definition and that that revision does not exist.
        // In that case we just get the latest one.
        if (classicReleasePipeline == null)
        {
            classicReleasePipeline = await _azdoClient.GetAsync(ReleaseManagement.Definition(
                runInfo.ProjectId, release.ReleaseDefinition.Id), runInfo.Organization);
        }

        var stages = classicReleasePipeline.Environments
            .Select(e => new StageReport { Id = e.Id.ToString(CultureInfo.InvariantCulture), Name = e.Name })
            .ToList();

        return new PipelineRunInfo(runInfo.Organization, project, classicReleasePipeline, stages, release,
            runInfo.StageId);
    }

    private async Task<PipelineRunInfo> EnrichWithBuildInfoAsync(PipelineRunInfo runInfo)
    {
        var build = await _azdoClient.GetAsync(Builds.Build(
            runInfo.ProjectId, runInfo.RunId), runInfo.Organization);

        if (build == null)
        {
            throw new InvalidOperationException(ErrorMessages.BuildNotAvailableErrorMessage(runInfo.RunId));
        }

        var buildDefinition = await _azdoClient.GetAsync(Builds.BuildDefinition(
            runInfo.ProjectId, build.Definition.Id, build.Definition.Revision), runInfo.Organization);

        var pipelineRunInfo =
            new PipelineRunInfo(runInfo.Organization, buildDefinition, null, null, build, runInfo.StageId);

        try
        {
            buildDefinition.YamlUsedInRun =
                await _azdoClient.GetAsStringAsync(Builds.GetLogs1(runInfo.ProjectId, build.Id), runInfo.Organization);
            pipelineRunInfo.Stages = GetPipelineStages(buildDefinition.YamlUsedInRun);
            pipelineRunInfo.PipelineType =
                await GetPipelineType(runInfo.Organization, buildDefinition, pipelineRunInfo.Stages);
        }
        catch (FlurlHttpException e) when (e?.Call?.HttpStatus == HttpStatusCode.BadRequest)
        {
            var json = await e.GetResponseStringAsync();
            if (!string.IsNullOrEmpty(json))
            {
                var errorMessage = json.ToJson().SelectToken("message")?.ToString();
                if (errorMessage != null)
                {
                    pipelineRunInfo.ErrorMessage = errorMessage;
                }
            }

            pipelineRunInfo.PipelineType = ItemTypes.InvalidYamlPipeline;
        }

        return pipelineRunInfo;
    }

    private static IList<StageReport> GetPipelineStages(string yamlUsedInRun)
    {
        if (string.IsNullOrEmpty(yamlUsedInRun))
        {
            return new List<StageReport>();
        }

        var stages = yamlUsedInRun.ToJson()
            .SelectTokens(_stagesSelector)
            .Select(x => new StageReport { Id = x.ToString(), Name = x.ToString() })
            .ToList();

        return stages;
    }

    private async Task<string> GetPipelineType(string organization, BuildDefinition buildPipelineDefinition,
        ICollection<StageReport> stages)
    {
        if (buildPipelineDefinition.Process.Type == PipelineProcessType.GuiPipeline)
        {
            return ItemTypes.ClassicBuildPipeline;
        }

        if (IsStagelessPipeline(stages))
        {
            return ItemTypes.StagelessYamlPipeline;
        }

        var compliancyReportDto = await _extensionDataRepository.DownloadAsync<CompliancyReportDto>(
            CompliancyScannerExtensionConstants.Publisher,
            CompliancyScannerExtensionConstants.Collection, _config.ExtensionName, organization,
            buildPipelineDefinition.Project.Name);

        if (compliancyReportDto?.BuildPipelines == null)
        {
            throw new ArgumentException($"Cannot be null: {nameof(compliancyReportDto.BuildPipelines)}");
        }

        // The default branch potentially does not contain stages. Therefore, the pipeline type in Hub is defined as 'stageless yaml'.
        return compliancyReportDto.BuildPipelines
            .Any(resourceReportDto => resourceReportDto.Id == buildPipelineDefinition.Id &&
                                      (resourceReportDto.Type == ItemTypes.YamlPipelineWithStages ||
                                       resourceReportDto.Type == ItemTypes.StagelessYamlPipeline))
            ? ItemTypes.YamlPipelineWithStages
            : ItemTypes.YamlReleasePipeline;
    }

    private static bool IsStagelessPipeline(ICollection<StageReport>? stages) =>
        stages == null || !stages.Any() || (stages.Count == 1 && stages.Single().Name != null &&
                                            stages.Single().Name!.Equals(_defaultStageName,
                                                StringComparison.InvariantCultureIgnoreCase));

    private static IEnumerable<string?>? GetProdStages(PipelineRunInfo runInfo,
        IEnumerable<PipelineRegistration> registrations)
    {
        var registeredProdStages = registrations.Where(r => r.IsProduction)
            .Select(r => r.StageId)
            .Distinct();

        var prodStages = runInfo.Stages?.Select(s => s.Id)
            .Where(s => registeredProdStages.Contains(s))
            .Select(s => s);

        return prodStages;
    }

    private async Task<IEnumerable<RuleCompliancyReport>> GetCompliancyForProjectPermissionRules(
        PipelineRunInfo runInfo, RuleProfile ruleProfile)
    {
        var ruleReport = new List<RuleCompliancyReport>();

        var projectRule = _projectRules.GetAllByRuleProfile(ruleProfile)
            .FirstOrDefault(x => x.Name == RuleNames.NobodyCanDeleteTheProject);
        if (projectRule == null)
        {
            return ruleReport;
        }

        var isCompliant = await projectRule.EvaluateAsync(runInfo.Organization, runInfo.ProjectId);
        ruleReport.Add(new RuleCompliancyReport
        {
            IsCompliant = isCompliant,
            HasDeviation = false,
            RuleDescription = projectRule.Description,
            ItemName = runInfo.ProjectName
        });

        return ruleReport;
    }

    private async Task<IEnumerable<RuleCompliancyReport>> GetCompliancyForClassicReleasePipelineRules(
        PipelineRunInfo runInfo,
        IEnumerable<PipelineRegistration> registrations, RuleProfile ruleProfile) =>
        await CheckClassicReleaseCompliancyRules(_classicReleasePipelineRules.GetAllByRuleProfile(ruleProfile), runInfo,
            registrations);

    private async Task<IEnumerable<RuleCompliancyReport>> GetCompliancyForYamlReleasePipelineRules(
        PipelineRunInfo runInfo,
        IEnumerable<PipelineRegistration> registrations, RuleProfile ruleProfile) =>
        await CheckYamlReleaseCompliancyRules(_yamlReleasePipelineRules.GetAllByRuleProfile(ruleProfile), runInfo,
            registrations);

    private async Task<IEnumerable<RuleCompliancyReport>> GetCompliancyForBuildPipelineRules(PipelineRunInfo runInfo,
        IEnumerable<PipelineRegistration> registrations, IEnumerable<BuildDefinition> buildPipelines,
        RuleProfile ruleProfile)
    {
        var buildPipelineRules = _buildPipelineRules.GetAllByRuleProfile(ruleProfile);
        return (await Task.WhenAll(buildPipelines
                .Select(async b => await CheckBuildCompliancyRules(buildPipelineRules, runInfo, registrations, b))))
            .SelectMany(b => b);
    }

    private async Task<IEnumerable<RuleCompliancyReport>> GetCompliancyForRepositoryRules(PipelineRunInfo runInfo,
        IEnumerable<PipelineRegistration> registrations, IEnumerable<Repository> repositories, RuleProfile ruleProfile)
    {
        var repositoryRules = _repositoryRules.GetAllByRuleProfile(ruleProfile);

        return (await Task.WhenAll(repositories
                .Select(async repository =>
                    await CheckRepositoryCompliancyRules(repositoryRules, runInfo, registrations, repository))))
            .SelectMany(r => r);
    }

    private async Task<IEnumerable<RuleCompliancyReport>> CheckClassicReleaseCompliancyRules(
        IEnumerable<IClassicReleasePipelineRule?> rules,
        PipelineRunInfo runInfo, IEnumerable<PipelineRegistration> registrations)
    {
        var ruleReport = new List<RuleCompliancyReport>();

        foreach (var rule in rules)
        {
            if (rule == null)
            {
                continue;
            }

            var isCompliant =
                await rule.EvaluateAsync(runInfo.Organization, runInfo.ProjectId, runInfo.ClassicReleasePipeline);
            var hasDeviation = (await Task.WhenAll(registrations
                    .Select(r => HasDeviationAsync(runInfo, r, rule.Name))))
                .All(x => x);

            ruleReport.Add(new RuleCompliancyReport
            {
                IsCompliant = isCompliant,
                HasDeviation = hasDeviation,
                RuleDescription = rule.Description,
                ItemName = runInfo.ClassicReleasePipeline?.Name
            });
        }

        return ruleReport;
    }

    private async Task<IEnumerable<RuleCompliancyReport>> CheckYamlReleaseCompliancyRules(
        IEnumerable<IYamlReleasePipelineRule?> rules,
        PipelineRunInfo runInfo, IEnumerable<PipelineRegistration> registrations)
    {
        var ruleReport = new List<RuleCompliancyReport>();

        foreach (var rule in rules)
        {
            if (rule == null)
            {
                continue;
            }

            var isCompliant = await rule.EvaluateAsync(runInfo.Organization, runInfo.ProjectId, runInfo.BuildPipeline);
            var hasDeviation = (await Task.WhenAll(registrations
                    .Select(r => HasDeviationAsync(runInfo, r, rule.Name))))
                .All(x => x);

            ruleReport.Add(new RuleCompliancyReport
            {
                IsCompliant = isCompliant,
                HasDeviation = hasDeviation,
                RuleDescription = rule.Description,
                ItemName = runInfo.BuildPipeline?.Name
            });
        }

        return ruleReport;
    }

    private async Task<IEnumerable<RuleCompliancyReport>> CheckBuildCompliancyRules(
        IEnumerable<IBuildPipelineRule?> rules,
        PipelineRunInfo runInfo, IEnumerable<PipelineRegistration> registrations, BuildDefinition buildPipeline)
    {
        var ruleReport = new List<RuleCompliancyReport>();

        foreach (var rule in rules)
        {
            if (rule == null)
            {
                continue;
            }

            var isCompliant = await rule.EvaluateAsync(runInfo.Organization, buildPipeline.Project.Id, buildPipeline);
            var hasDeviation = (await Task.WhenAll(registrations
                    .Select(r => HasDeviationAsync(runInfo, r, buildPipeline, rule.Name))))
                .All(x => x);

            ruleReport.Add(new RuleCompliancyReport
            {
                IsCompliant = isCompliant,
                HasDeviation = hasDeviation,
                RuleDescription = rule.Description,
                ItemName = buildPipeline.Name
            });
        }

        return ruleReport;
    }

    private async Task<IEnumerable<RuleCompliancyReport>> CheckRepositoryCompliancyRules(
        IEnumerable<IRepositoryRule?> rules,
        PipelineRunInfo runInfo, IEnumerable<PipelineRegistration> registrations, Repository repository)
    {
        var ruleReport = new List<RuleCompliancyReport>();

        foreach (var rule in rules)
        {
            if (rule == null)
            {
                continue;
            }

            var project = new Project
            {
                Id = runInfo.ProjectId,
                Name = runInfo.ProjectName
            };

            var repoProjectId = string.IsNullOrEmpty(repository.Project?.Id)
                ? await _repoService.GetProjectIdByNameAsync(runInfo.Organization, project, repository)
                : repository.Project.Id;

            var isCompliant = await rule.EvaluateAsync(runInfo.Organization, repoProjectId, repository.Id);
            var hasDeviation = (await Task.WhenAll(registrations
                    .Select(r => HasDeviationAsync(runInfo, r, repository, repoProjectId, rule.Name))))
                .All(x => x);

            ruleReport.Add(new RuleCompliancyReport
            {
                IsCompliant = isCompliant,
                HasDeviation = hasDeviation,
                RuleDescription = rule.Description,
                ItemName = repository.Name
            });
        }

        return ruleReport;
    }

    private async Task<bool> HasDeviationAsync(PipelineRunInfo runInfo, PipelineRegistration registration,
        string ruleName)
    {
        var deviations = await GetDeviationsAsync(runInfo);

        if (deviations == null)
        {
            return false;
        }

        return deviations.Any(d => d.ItemId == runInfo.PipelineId &&
                                   d.RuleName == ruleName &&
                                   d.CiIdentifier == registration.CiIdentifier);
    }

    private async Task<bool> HasDeviationAsync(PipelineRunInfo runInfo, PipelineRegistration registration,
        BuildDefinition buildPipeline,
        string ruleName)
    {
        var deviations = await GetDeviationsAsync(runInfo);

        if (deviations == null)
        {
            return false;
        }

        return deviations.Any(d => d.ItemId == buildPipeline.Id &&
                                   d.RuleName == ruleName &&
                                   d.CiIdentifier == registration.CiIdentifier &&
                                   ((d.ForeignProjectId == null && d.ProjectId == buildPipeline.Project.Id) ||
                                    d.ForeignProjectId == buildPipeline.Project.Id));
    }

    private async Task<bool> HasDeviationAsync(PipelineRunInfo runInfo, PipelineRegistration registration,
        Repository repository,
        string repoProjectId, string ruleName)
    {
        var deviations = await GetDeviationsAsync(runInfo);

        if (deviations == null)
        {
            return false;
        }

        return deviations.Any(d => d.ItemId == repository.Id &&
                                   d.RuleName == ruleName &&
                                   d.CiIdentifier == registration.CiIdentifier &&
                                   ((d.ForeignProjectId == null && d.ProjectId == repoProjectId) ||
                                    d.ForeignProjectId == repoProjectId));
    }

    private async Task<IList<Deviation>?> GetDeviationsAsync(PipelineRunInfo runInfo) =>
        await _deviationRepo.GetListAsync(runInfo.Organization, runInfo.ProjectId);
}