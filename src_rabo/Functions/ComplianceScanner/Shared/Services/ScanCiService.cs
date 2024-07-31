using Rabobank.Compliancy.Core.PipelineResources.Services;
using Rabobank.Compliancy.Domain.RuleProfiles;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Helpers;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;

public class ScanCiService : IScanCiService
{
    private readonly IAzdoRestClient _azdoClient;
    private readonly IPipelinesService _pipelinesService;
    private readonly IScanItemsService _scanItemsService;
    private readonly IVerifyComplianceService _verifyComplianceService;
    private readonly ComplianceConfig _config;
    private readonly IReleasePipelineService _releasePipelineService;
    private readonly IBuildPipelineService _buildPipelineService;

    public ScanCiService(
        IAzdoRestClient azdoClient,
        IPipelinesService pipelinesService,
        IScanItemsService scanItemsService,
        IVerifyComplianceService verifyComplianceService,
        ComplianceConfig config,
        IReleasePipelineService releasePipelineService,
        IBuildPipelineService buildPipelineService)
    {
        _azdoClient = azdoClient;
        _pipelinesService = pipelinesService;
        _scanItemsService = scanItemsService;
        _verifyComplianceService = verifyComplianceService;
        _config = config;
        _releasePipelineService = releasePipelineService;
        _buildPipelineService = buildPipelineService;
    }

    public async Task<CiReport> ScanCiAsync(string organization, Project project, string ciIdentifier, DateTime scanDate, IEnumerable<PipelineRegistration> pipelineRegistrations)
    {
        var pipelineRegistrationsForThisCi = pipelineRegistrations.Where(registration => registration.CiIdentifier == ciIdentifier);

        if (!pipelineRegistrationsForThisCi.Any())
        {
            return CreateEmptyCIReport(organization, project.Id, ciIdentifier, scanDate, new Exception($"There are no pipeline registrations for CI: {ciIdentifier}"));
        }

        var allClassicReleasePipelines = await _pipelinesService.GetClassicReleasePipelinesAsync(organization, project.Id, pipelineRegistrations);

        // Get all Classic Release Pipelines for which a Registration Exists with this CI Identifier and which has at least one existing stage (pipeline.environment) registered
        var classicReleasePipelinesForCiWithValidRegistration = allClassicReleasePipelines.Where(pipeline =>
            pipeline.PipelineRegistrations.Any(registration =>
                registration.CiIdentifier == ciIdentifier &&
                pipeline.Environments.Select(e => e.Id).Contains(registration.GetStageIdAsNullableInt() ?? -1)));

        var allYamlPipelines = await _pipelinesService.GetAllYamlPipelinesAsync(organization, project.Id, pipelineRegistrations);

        // Get all YAML Release Pipelines for which a Registration Exists with this CI Identifier and which has at least one existing stage registered
        var yamlReleasePipelinesForCiWithValidRegistration = allYamlPipelines.Where(pipeline =>
            pipeline.PipelineType == ItemTypes.YamlPipelineWithStages && // Only a YAML pipeline with stages can be considered a Release pipeline
            pipeline.PipelineRegistrations.Any(registration =>
                registration.CiIdentifier == ciIdentifier &&
                pipeline.Stages.Select(s => s.Id).Contains(registration.StageId, StringComparer.OrdinalIgnoreCase)));

        if (!classicReleasePipelinesForCiWithValidRegistration.Any() && !yamlReleasePipelinesForCiWithValidRegistration.Any())
        {
            return CreateEmptyCIReport(organization, project.Id, ciIdentifier, scanDate, new Exception($"There are no release pipelines for CI: {ciIdentifier}"));
        }

        var allBuildPipelines = (await _pipelinesService.GetClassicBuildPipelinesAsync(organization, project.Id)).Concat(allYamlPipelines);

        var principleReports = await ScanCi(organization, project, classicReleasePipelinesForCiWithValidRegistration, yamlReleasePipelinesForCiWithValidRegistration,
            scanDate, allBuildPipelines, ciIdentifier);

        return CreateCiReport(organization, project, ciIdentifier, scanDate, pipelineRegistrationsForThisCi, principleReports);
    }

    public async Task<NonProdCompliancyReport> ScanNonProdPipelineAsync(string organization, Project project, DateTime scanDate, string nonProdPipelineId, IEnumerable<PipelineRegistration> pipelineRegistrations)
    {
        var allClassicReleasePipelines = await _pipelinesService.GetClassicReleasePipelinesAsync(organization, project.Id, pipelineRegistrations);

        var allYamlPipelines = await _pipelinesService.GetAllYamlPipelinesAsync(organization, project.Id, pipelineRegistrations);

        var allYamlReleasePipelines = allYamlPipelines.Where(x => x.PipelineType == ItemTypes.YamlPipelineWithStages);

        var allBuildPipelines = (await _pipelinesService.GetClassicBuildPipelinesAsync(organization, project.Id)).Concat(allYamlPipelines);

        var nonProdYamlPipelineForThisScan = allYamlReleasePipelines.Where(p => p.Id == nonProdPipelineId);
        var nonProdClassicPipelineForThisScan = allClassicReleasePipelines.Where(p => p.Id == nonProdPipelineId);

        if (!nonProdYamlPipelineForThisScan.Any() && !nonProdClassicPipelineForThisScan.Any())
        {
            return default;
        }

        var pipelineType = nonProdYamlPipelineForThisScan.Any() ? ItemTypes.YamlReleasePipeline : ItemTypes.ClassicReleasePipeline;

        var principleReports = await ScanCi(organization, project, nonProdClassicPipelineForThisScan, nonProdYamlPipelineForThisScan, scanDate, allBuildPipelines, null);

        return new NonProdCompliancyReport
        {
            Date = scanDate,
            PrincipleReports = principleReports,
            RescanUrl = CreateUrl.NonProdPipelineRescanUrl(_config, organization, project.Id, nonProdPipelineId),
            IsCompliant = principleReports.All(p => p.IsCompliant),
            PipelineId = nonProdPipelineId,
            PipelineName = nonProdYamlPipelineForThisScan?.Select(m => m.Name).FirstOrDefault() ?? nonProdClassicPipelineForThisScan?.Select(m => m.Name).FirstOrDefault(),
            PipelineType = pipelineType
        };
    }

    private async Task<IEnumerable<PrincipleReport>> ScanCi(string organization, Project project,
        IEnumerable<ReleaseDefinition> classicReleasePipelinesForCi, IEnumerable<BuildDefinition> yamlReleasePipelinesForCi,
        DateTime scanDate, IEnumerable<BuildDefinition> allBuildPipelines, string ciIdentifier)
    {
        // Fetch the actual list of environments. We do this late in the process, because its a very consuming
        // operation and here we know we filtered every irrelevant pipeline out.
        foreach (var pipeline in classicReleasePipelinesForCi)
        {
            var pipelineDetailed = await _azdoClient.GetAsync(Infra.AzdoClient.Requests.ReleaseManagement.Definition(project.Id, pipeline.Id), organization);
            pipeline.Environments = pipelineDetailed.Environments;
        }

        var buildPipelinesClassicRelease = (await System.Threading.Tasks.Task.WhenAll(classicReleasePipelinesForCi
                .Select(async r => await _releasePipelineService.GetLinkedPipelinesAsync(organization, r, project.Id, allBuildPipelines))))
            .SelectMany(b => b);

        var buildPipelinesYamlRelease = (await System.Threading.Tasks.Task.WhenAll(yamlReleasePipelinesForCi
                .Select(async r => await GetBuildPipelinesForYamlRelease(organization, r, allBuildPipelines))))
            .SelectMany(b => b);

        var buildPipelines = buildPipelinesClassicRelease
            .Concat(buildPipelinesYamlRelease)
            .Distinct();

        var repositories = await _releasePipelineService.GetLinkedRepositoriesAsync(organization,
            classicReleasePipelinesForCi, yamlReleasePipelinesForCi.Concat(buildPipelines));

        var results = await ScanItemsForCiAsync(organization, project, classicReleasePipelinesForCi,
            yamlReleasePipelinesForCi, buildPipelines, repositories, ciIdentifier);

        return _verifyComplianceService.CreatePrincipleReports(results, scanDate);
    }

    private async Task<IEnumerable<BuildDefinition>> GetBuildPipelinesForYamlRelease(string organization, BuildDefinition yamlReleasePipeline,
        IEnumerable<BuildDefinition> allBuildPipelines)
    {
        var result = await _buildPipelineService.GetLinkedPipelinesAsync(organization, yamlReleasePipeline, allBuildPipelines);

        if (!result.Any())
        {
            return new List<BuildDefinition> { yamlReleasePipeline }; // YamlRelease is used for CI & CD
        }

        return result;
    }

    private async Task<IEnumerable<EvaluatedRule>> ScanItemsForCiAsync(string organization,
        Project project, IEnumerable<ReleaseDefinition> classicReleasePipelines,
        IEnumerable<BuildDefinition> yamlReleasePipelines, IEnumerable<BuildDefinition> buildPipelines,
        IEnumerable<Repository> repositories, string ciIdentifier)
    {
        var allUsedProfilesForCi = new List<RuleProfile>();

        if (classicReleasePipelines != null)
        {
            allUsedProfilesForCi.AddRange(classicReleasePipelines
                .SelectMany(r => r.PipelineRegistrations.Select(r => r.GetRuleProfile()).DistinctBy(r => r.Name)));
        }
        if (yamlReleasePipelines != null)
        {
            allUsedProfilesForCi.AddRange(yamlReleasePipelines
                .SelectMany(r => r.PipelineRegistrations.Select(r => r.GetRuleProfile()).DistinctBy(r => r.Name)));
        }

        var projectResults = await _scanItemsService.ScanProjectAsync(
            organization, project, ciIdentifier);
        var repositoriesResults = await _scanItemsService.ScanRepositoriesAsync(
            organization, project, repositories, ciIdentifier);
        var buildPipelinesResults = await _scanItemsService.ScanBuildPipelinesAsync(
            organization, project, buildPipelines, ciIdentifier, allUsedProfilesForCi);
        var yamlReleasePipelinesResults = await _scanItemsService.ScanYamlReleasePipelinesAsync(
            organization, project, yamlReleasePipelines, ciIdentifier);
        var classicReleasePipelineResults = await _scanItemsService.ScanClassicReleasePipelinesAsync(
            organization, project, classicReleasePipelines, ciIdentifier);

        return projectResults
            .Concat(repositoriesResults)
            .Concat(buildPipelinesResults)
            .Concat(yamlReleasePipelinesResults)
            .Concat(classicReleasePipelineResults)
            //The GroupBy and Select are required, because YAML CiCd pipelines are scanned twice.
            //The NobodyCanDeleteBuilds rule is executed for both build pipelines and YAML release pipelines categories.
            .GroupBy(r => new { r.Name, r.Item.Link })
            .Select(g => g.Last());
    }

    private CiReport CreateCiReport(string organization, Project project, string ciIdentifier,
        DateTime scanDate, IEnumerable<PipelineRegistration> pipelineRegistrations,
        IEnumerable<PrincipleReport> principleReports)
    {
        var pipelineRegistration = pipelineRegistrations.First();
        return new CiReport(ciIdentifier, pipelineRegistration.CiName, scanDate)
        {
            AssignmentGroup = pipelineRegistration.AssignmentGroup,
            IsSOx = pipelineRegistration.IsSoxApplication,
            PrincipleReports = principleReports,
            RescanUrl = CreateUrl.CiRescanUrl(_config, organization, project.Id, ciIdentifier),
            AicRating = pipelineRegistration.AicRating,
            CiSubtype = pipelineRegistration.CiSubtype
        };
    }

    private CiReport CreateEmptyCIReport(string organization, string projectId, string ciIdentifier, DateTime scanDate, Exception exception) => 
        new(ciIdentifier, null, scanDate)
    {
        IsScanFailed = true,
        RescanUrl = CreateUrl.CiRescanUrl(_config, organization, projectId, ciIdentifier),
        ScanException = new ExceptionSummaryReport(exception),
        PrincipleReports = Enumerable.Empty<PrincipleReport>()
    };
}