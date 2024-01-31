#nullable enable

using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Extensions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Helpers;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.AzdoClient.Response.Interfaces;
using Rabobank.Compliancy.Infra.StorageClient;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;

public class ScanProjectService : IScanProjectService
{
    private readonly IAzdoRestClient _azdoClient;
    private readonly ICompliancyReportService _compliancyReportService;
    private readonly ComplianceConfig _config;
    private readonly IPipelinesService _pipelinesService;
    private readonly IPipelineRegistrationRepository _registrationRepo;
    private readonly IScanCiService _scanCiService;

    public ScanProjectService(
        IAzdoRestClient azdoClient,
        IPipelineRegistrationRepository registrationRepo,
        IPipelinesService pipelinesService,
        IScanCiService scanCiService,
        ICompliancyReportService compliancyReportService,
        ComplianceConfig config)
    {
        _azdoClient = azdoClient;
        _registrationRepo = registrationRepo;
        _pipelinesService = pipelinesService;
        _scanCiService = scanCiService;
        _compliancyReportService = compliancyReportService;
        _config = config;
    }

    /// <summary>
    ///     This method creates the compliancy report for a project. This report contains all information for
    ///     the frontend. You can see the different reports on the compliancy hub.
    /// </summary>
    /// <param name="organization"></param>
    /// <param name="project"></param>
    /// <param name="scanDate"></param>
    /// <param name="parallelCiScans"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<CompliancyReport> ScanProjectAsync(string organization, Project project,
        DateTime scanDate, int parallelCiScans)
    {
        // First, get all registrations and pipelines from the table storage and Azure Devops API
        var pipelineRegistrations = await _registrationRepo.GetAsync(organization, project.Id);
        var classicReleasePipelines =
            await _pipelinesService.GetClassicReleasePipelinesAsync(organization, project.Id, pipelineRegistrations);
        var allYamlPipelines =
            await _pipelinesService.GetAllYamlPipelinesAsync(organization, project.Id, pipelineRegistrations);
        var yamlReleasePipelines = allYamlPipelines.Where(x => x.PipelineType == ItemTypes.YamlPipelineWithStages);

        // Next: prepare all lists for scanning and report generating.
        // - General
        var ciIdentifiersToBeScanned = new HashSet<string>();
        // - YAML
        var registeredYamlReleasePipelines = new List<BuildDefinition>();
        var unregisteredYamlReleasePipelines = new List<BuildDefinition>();
        var registeredYamlReleasePipelinesMissingStage = new List<BuildDefinition>();
        var nonProdYamlReleasePipelines = new List<BuildDefinition>();
        // - Classic
        var registeredClassicReleasePipelines = new List<ReleaseDefinition>();
        var unregisteredClassicReleasePipelines = new List<ReleaseDefinition>();
        var registeredClassicReleasePipelinesMissingStage = new List<ReleaseDefinition>();
        var nonProdClassicReleasePipelines = new List<ReleaseDefinition>();

        // Then fill all prepared lists looping through the pipelines and their registrations only once in the following methods
        FilterYamlPipelines(yamlReleasePipelines, ciIdentifiersToBeScanned, registeredYamlReleasePipelines,
            unregisteredYamlReleasePipelines, nonProdYamlReleasePipelines, registeredYamlReleasePipelinesMissingStage);
        FilterClassicPipelines(classicReleasePipelines, ciIdentifiersToBeScanned, registeredClassicReleasePipelines,
            unregisteredClassicReleasePipelines, nonProdClassicReleasePipelines,
            registeredClassicReleasePipelinesMissingStage);

        // Scan all projects based on the ciIdentifiers found in the previous filter methods
        var registeredCiReports = await CreateRegisteredCiReportsAsync(organization, project,
            scanDate, ciIdentifiersToBeScanned, pipelineRegistrations, parallelCiScans);

        // Sometimes during the batch scan NullReferenceExceptions are thrown on random projects/organizations.
        // For some reason it seems like the CiReport is null. To check if this is the case
        // an Exception is now thrown so this will be logged in LogAnalytics.
        if (registeredCiReports == null)
        {
            throw new ArgumentException($"Error occurred while scanning project {project.Id}. CiReport is null.");
        }

        // After scanning and checking for errors, we retrieve all pipeline ID's that are used as a source by other pipelines
        // These are considered build pipelines and do not belong in the Unregistered-, Registered-, and MissingProdStage reports.
        var idsOfPipelinesConsumedAsBuild =
            GetItemIdsFromReport(registeredCiReports, ItemTypes.BuildPipeline, project.Id);

        // Create the report for unregistered pipelines
        var unregisteredPipelineReports = CreateUnregisteredPipelineReports(organization, project.Id,
            unregisteredClassicReleasePipelines, unregisteredYamlReleasePipelines, idsOfPipelinesConsumedAsBuild);

        // Create the report for registered pipelines with missing prod stages (pipelines consumed by other pipelines are filtered out)
        var registeredPipelinesMissingProdStageReports = CreateRegisteredPipelinesMissingProdStageReports(organization,
            project.Id,
            registeredClassicReleasePipelinesMissingStage, registeredYamlReleasePipelinesMissingStage,
            idsOfPipelinesConsumedAsBuild);

        // Create the report for registered pipelines (pipelines consumed by other pipelines are filtered out)
        var registeredPipelineReports = CreateRegisteredPipelineReports(organization, project.Id,
            registeredClassicReleasePipelines, registeredYamlReleasePipelines,
            nonProdClassicReleasePipelines, nonProdYamlReleasePipelines, idsOfPipelinesConsumedAsBuild);

        // Create the report for non-prod pipelines
        var nonProdPipelineReports = await CreateNonProdCompliancyReportsAsync(organization, project,
            scanDate, nonProdYamlReleasePipelines, nonProdClassicReleasePipelines, pipelineRegistrations,
            parallelCiScans);

        // In order to create the report for build pipelines, first fetch the classic build pipelines and concat these with all of the yaml pipelines
        var allBuildAndYamlPipelines = (await _pipelinesService.GetClassicBuildPipelinesAsync(
            organization, project.Id)).Concat(allYamlPipelines);

        // then create the report (pipelines consumed by other pipelines are used here)
        var buildPipelineReports = CreateBuildPipelineReports(organization, project.Id,
            registeredCiReports, allBuildAndYamlPipelines, idsOfPipelinesConsumedAsBuild);

        // Create the report for the repositories
        var repositoryReports = await CreateRepositoryReportsAsync(organization, project.Id, registeredCiReports);

        // Put everything in the CompliancyReport object returned to the frontend.
        var complianceReport = new CompliancyReport
        {
            Id = project.Name,
            Date = scanDate,
            RescanUrl = CreateUrl.ProjectRescanUrl(_config, organization, project.Id),
            HasReconcilePermissionUrl = CreateUrl.HasPermissionUrl(
                _config, organization, project.Id),
            UnregisteredPipelines = unregisteredPipelineReports.OrderBy(p => p.Name).ToList(),
            RegisteredPipelinesNoProdStage = registeredPipelinesMissingProdStageReports.OrderBy(p => p.Name).ToList(),
            RegisteredConfigurationItems = registeredCiReports.OrderBy(c => c.Name).ToList(),
            RegisteredPipelines = registeredPipelineReports.OrderBy(p => p.Name).ToList(),
            BuildPipelines = buildPipelineReports.OrderBy(p => p.Name).ToList(),
            Repositories = repositoryReports.OrderBy(p => p.Name).ToList(),
            NonProdPipelinesRegisteredForScan = nonProdPipelineReports.OrderBy(p => p.PipelineName).ToList()
        };

        await _compliancyReportService.UpdateComplianceReportAsync(organization, Guid.Parse(project.Id),
            complianceReport, scanDate);

        return complianceReport;
    }

    private static void FilterYamlPipelines(IEnumerable<BuildDefinition> yamlReleasePipelines,
        ISet<string> ciIdentifiersToBeScanned,
        ICollection<BuildDefinition> registeredYamlReleasePipelines,
        ICollection<BuildDefinition> unregisteredYamlReleasePipelines,
        ICollection<BuildDefinition> nonProdYamlReleasePipelines,
        ICollection<BuildDefinition> registeredYamlReleasePipelinesMissingStage)
    {
        foreach (var pipeline in yamlReleasePipelines)
        {
            // If this pipeline is not registered, add it to the corresponding list and continue to the next.
            if (pipeline.PipelineRegistrations == null || !pipeline.PipelineRegistrations.Any())
            {
                unregisteredYamlReleasePipelines.Add(pipeline);
                continue;
            }

            var registrationsProd = pipeline.PipelineRegistrations.Where(registration => registration.IsProduction);

            // If none of the pipeline registrations concerning this pipeline are production, add it to the corresponding list and continue to the next.
            if (!registrationsProd.Any())
            {
                nonProdYamlReleasePipelines.Add(pipeline);
                continue;
            }

            // For each production registration which concern this pipeline, get CiIdentifiers of those where the registered stage exists in the current pipeline
            var ciIdentifiersWithCorrespondingStages = registrationsProd.Where(registration =>
                pipeline.GetStageIds().Contains(registration.StageId, StringComparer.OrdinalIgnoreCase)
            ).Select(registration => registration.CiIdentifier);

            ciIdentifiersToBeScanned.UnionWith(ciIdentifiersWithCorrespondingStages);

            // If there are no CiIdentifiers found in the above WhereClause, this pipeline is Prod and Registered, but without containing a registered prod stage
            if (!ciIdentifiersWithCorrespondingStages.Any())
            {
                registeredYamlReleasePipelinesMissingStage.Add(pipeline);
                continue;
            }

            // After all checks and continues, the pipeline must be registered with valid stages.
            registeredYamlReleasePipelines.Add(pipeline);
        }
    }

    private static void FilterClassicPipelines(IEnumerable<ReleaseDefinition> classicReleasePipelines,
        ISet<string> ciIdentifiersToBeScanned,
        ICollection<ReleaseDefinition> registeredClassicReleasePipelines,
        ICollection<ReleaseDefinition> unregisteredClassicReleasePipelines,
        ICollection<ReleaseDefinition> nonProdClassicReleasePipelines,
        ICollection<ReleaseDefinition> registeredClassicReleasePipelinesMissingStage)
    {
        foreach (var pipeline in classicReleasePipelines)
        {
            // If this pipeline is not registered, add it to the corresponding list and continue to the next.
            if (pipeline.PipelineRegistrations == null || !pipeline.PipelineRegistrations.Any())
            {
                unregisteredClassicReleasePipelines.Add(pipeline);
                continue;
            }

            var registrationsProd = pipeline.PipelineRegistrations.Where(registration => registration.IsProduction);

            // If none of the pipeline registrations concerning this pipeline are production, add it to the corresponding list and continue to the next.
            if (!registrationsProd.Any())
            {
                nonProdClassicReleasePipelines.Add(pipeline);
                continue;
            }

            // For each production registration which concern this pipeline, get CiIdentifiers of those where the registered stage exists in the current pipeline
            var ciIdentifiersWithCorrespondingStages = registrationsProd.Where(registration =>
                pipeline.GetStageIds().Contains(registration.StageId, StringComparer.OrdinalIgnoreCase)
            ).Select(registration => registration.CiIdentifier);

            ciIdentifiersToBeScanned.UnionWith(ciIdentifiersWithCorrespondingStages);

            // If there are no CiIdentifiers found in the above WhereClause, this pipeline is Prod and Registered, but without containing a registered prod stage
            if (!ciIdentifiersWithCorrespondingStages.Any())
            {
                registeredClassicReleasePipelinesMissingStage.Add(pipeline);
                continue;
            }

            // After all checks and continues, the pipeline must be registered with valid stages.
            registeredClassicReleasePipelines.Add(pipeline);
        }
    }

    private async Task<IEnumerable<CiReport>> CreateRegisteredCiReportsAsync(string organization,
        Project project, DateTime scanDate, IEnumerable<string> ciIdentifiers,
        IEnumerable<PipelineRegistration> pipelineRegistrations, int parallelCiScans)
    {
        return await Task.WhenAll(ciIdentifiers
            .Select(async ciIdentifier =>
            {
                var semaphoreSlim = new SemaphoreSlim(parallelCiScans);
                await semaphoreSlim.WaitAsync();
                try
                {
                    return await _scanCiService.ScanCiAsync(organization, project, ciIdentifier, scanDate,
                        pipelineRegistrations);
                }
                catch (Exception e)
                {
                    return new CiReport(ciIdentifier, null, scanDate)
                    {
                        IsScanFailed = true,
                        RescanUrl = CreateUrl.CiRescanUrl(_config, organization, project.Id, ciIdentifier),
                        ScanException = new ExceptionSummaryReport(e),
                        PrincipleReports = Enumerable.Empty<PrincipleReport>()
                    };
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }));
    }

    private async Task<IEnumerable<NonProdCompliancyReport>> CreateNonProdCompliancyReportsAsync(string organization,
        Project project, DateTime scanDate, IEnumerable<IRegisterableDefinition> nonProdYamlPipelines,
        IEnumerable<IRegisterableDefinition> nonProdClassicPipelines,
        IEnumerable<PipelineRegistration> pipelineRegistrations, int parallelCiScans)
    {
        var nonProdPipelinesConcatenation =
            nonProdYamlPipelines.Concat(nonProdClassicPipelines); // Concat all non-prod pipelines together.
        return (await Task.WhenAll(nonProdPipelinesConcatenation
            .Where(pipeline =>
                pipeline.PipelineRegistrations.Any(registration =>
                    registration.ToBeScanned ==
                    true)) // Only scan non-prod pipelines with registrations "to be scanned".
            .Select(async pipeline =>
            {
                var semaphoreSlim = new SemaphoreSlim(parallelCiScans);
                await semaphoreSlim.WaitAsync();
                try
                {
                    return await _scanCiService.ScanNonProdPipelineAsync(organization, project,
                        scanDate, pipeline.Id, pipelineRegistrations);
                }
                catch (Exception e)
                {
                    return new NonProdCompliancyReport
                    {
                        Date = scanDate,
                        IsScanFailed = true,
                        RescanUrl = null,
                        ScanException = new ExceptionSummaryReport(e),
                        PrincipleReports = Enumerable.Empty<PrincipleReport>()
                    };
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }))).Where(report => report != null);
    }

    private IEnumerable<PipelineReport> CreateUnregisteredPipelineReports(string organization,
        string projectId, IEnumerable<ReleaseDefinition> unregisteredClassicReleasePipelines,
        IEnumerable<BuildDefinition> unregisteredYamlReleasePipelines,
        IEnumerable<string> idsOfPipelinesConsumedAsBuild)
    {
        var unregisteredClassicReleasePipelinesReport = unregisteredClassicReleasePipelines
            .Select(pipeline => ToPipelineReport(organization, projectId, pipeline, null));

        // Create the report, but first filter out all pipelines that are consumed by other builds, they are considered build-pipelines
        var unregisteredYamlPipelinesReport = unregisteredYamlReleasePipelines
            .Where(pipeline => !idsOfPipelinesConsumedAsBuild.Contains(pipeline.Id))
            .Select(pipeline => ToPipelineReport(organization, projectId, pipeline, null));

        return unregisteredClassicReleasePipelinesReport.Concat(unregisteredYamlPipelinesReport);
    }

    private IEnumerable<PipelineReport> CreateRegisteredPipelinesMissingProdStageReports(string organization,
        string projectId, IEnumerable<ReleaseDefinition> registeredClassicReleasePipelinesMissingStage,
        IEnumerable<BuildDefinition> registeredYamlReleasePipelinesMissingStage,
        IEnumerable<string> idsOfPipelinesConsumedAsBuild)
    {
        var registeredClassicPipelinesMissingStageReports = registeredClassicReleasePipelinesMissingStage
            .Select(pipeline => ToPipelineReport(organization, projectId, pipeline, null,
                GetCiInformation(pipeline),
                pipeline.PipelineRegistrations.Select(registration => registration.StageId)));

        // Create the report, but first filter out all pipelines that are consumed by other builds, they are considered build-pipelines
        var registeredYamlPipelinesMissingStageReports = registeredYamlReleasePipelinesMissingStage
            .Where(pipeline => !idsOfPipelinesConsumedAsBuild.Contains(pipeline.Id))
            .Select(pipeline => ToPipelineReport(organization, projectId, pipeline, null,
                GetCiInformation(pipeline),
                pipeline.PipelineRegistrations.Select(registration => registration.StageId)));

        return registeredClassicPipelinesMissingStageReports
            .Concat(registeredYamlPipelinesMissingStageReports);
    }

    private IEnumerable<PipelineReport> CreateRegisteredPipelineReports(
        string organization, string projectId, IEnumerable<ReleaseDefinition> registeredProdClassicReleasePipelines,
        IEnumerable<BuildDefinition> registeredProdYamlReleasePipelines,
        IEnumerable<ReleaseDefinition> registeredNonProdClassicReleasePipelines,
        IEnumerable<BuildDefinition> registeredNonProdYamlReleasePipelines,
        IEnumerable<string> idsOfPipelinesConsumedAsBuild)
    {
        var prodClassicReleasePipelinesReport = registeredProdClassicReleasePipelines
            .Select(pipeline => ToPipelineReport(organization, projectId, pipeline, true,
                GetCiInformation(pipeline),
                GetRegisteredStageNames(pipeline)));
        var prodYamlReleasePipelinesReport = registeredProdYamlReleasePipelines
            .Select(pipeline => ToPipelineReport(organization, projectId, pipeline, true,
                GetCiInformation(pipeline),
                GetRegisteredStageNames(pipeline)));

        var nonProdClassicReleasePipelinesReport = registeredNonProdClassicReleasePipelines
            .Select(pipeline => ToPipelineReport(organization, projectId, pipeline, false,
                Enumerable.Empty<(string, string, string)>(),
                GetRegisteredStageNames(pipeline)));

        // Create the report, but first filter out all pipelines that are consumed by other builds, they are considered build-pipelines
        var nonProdYamlPipelinesReport = registeredNonProdYamlReleasePipelines
            .Where(x => !idsOfPipelinesConsumedAsBuild.Contains(x.Id))
            .Select(pipeline => ToPipelineReport(organization, projectId, pipeline, false,
                Enumerable.Empty<(string, string, string)>(),
                GetRegisteredStageNames(pipeline)));

        return prodClassicReleasePipelinesReport
            .Concat(prodYamlReleasePipelinesReport)
            .Concat(nonProdClassicReleasePipelinesReport)
            .Concat(nonProdYamlPipelinesReport);
    }

    private IEnumerable<ResourceReport> CreateBuildPipelineReports(string organization, string projectId,
        IEnumerable<CiReport> ciReports, IEnumerable<BuildDefinition> buildPipelines,
        IEnumerable<string> idsOfPipelinesConsumedAsBuild)
    {
        // This report has all build pipelines. This concerns the following types:
        // Classic Build
        // Yaml Stageless
        // Yaml Invalid

        // The Yaml Release pipelines are by definition pipelines with stages (Yaml with stages).
        // We do NOT want those on this report unless they are used as a source in other registered prod release pipelines.

        // First create a list of ID's of pipelines that are Yaml Release Pipelines
        // (we already have the ID's of yaml pipelines used by other pipelines, this is parameter prodBuildPipelineIds)
        var prodYamlReleasePipelineIds = GetItemIdsFromReport(ciReports, ItemTypes.YamlReleasePipeline, projectId);

        return buildPipelines // Only use pipelines from this list that are either:
            .Where(pipeline =>
                pipeline.PipelineType != ItemTypes.YamlPipelineWithStages || // NOT yaml release pipelines OR
                (idsOfPipelinesConsumedAsBuild.Contains(pipeline.Id) && // Consumed by other pipelines and
                 !prodYamlReleasePipelineIds
                     .Contains(pipeline.Id))) // not yaml release pipelines by CI-report definition

            // And then create the report for this selection.
            .Select(buildDefinition => CreateResourceReport(
                buildDefinition,
                organization,
                projectId,
                idsOfPipelinesConsumedAsBuild,
                ciReports));
    }

    private ResourceReport CreateResourceReport(BuildDefinition buildDefinition, string organization,
        string projectId, IEnumerable<string> idsOfPipelinesConsumedAsBuild, IEnumerable<CiReport> ciReports) =>
        new()
        {
            Id = buildDefinition.Id,
            Name = buildDefinition.Name,
            Type = buildDefinition.PipelineType,
            Link = buildDefinition.Links.Web.Href.AbsoluteUri,
            IsProduction = idsOfPipelinesConsumedAsBuild.Contains(buildDefinition.Id),
            CiIdentifiers = GetCiIdentifiers(buildDefinition.Id, ItemTypes.BuildPipeline, ciReports, projectId),
            OpenPermissionsUrl = CreateUrl.OpenPermissionsUrl(
                _config, organization, projectId, ItemTypes.BuildPipeline, buildDefinition.Id),
            DocumentationUrl = buildDefinition.PipelineType == ItemTypes.InvalidYamlPipeline
                ? CreateUrl.InvalidPipelineDocumentationUrl()
                : null
        };

    private ResourceReport CreateResourceReport(Repository repository, string organization, string projectId,
        IEnumerable<string> prodRepositoryIds, IEnumerable<CiReport> ciReports) =>
        new()
        {
            Id = repository.Id,
            Name = repository.Name,
            Type = ItemTypes.Repository,
            Link = repository.WebUrl.AbsoluteUri,
            IsProduction = prodRepositoryIds.Contains(repository.Id),
            CiIdentifiers = GetCiIdentifiers(repository.Id, ItemTypes.Repository, ciReports, projectId),
            OpenPermissionsUrl = CreateUrl.OpenPermissionsUrl(
                _config, organization, projectId, ItemTypes.Repository, repository.Id)
        };

    private async Task<IEnumerable<ResourceReport>> CreateRepositoryReportsAsync(
        string organization, string projectId, IEnumerable<CiReport> ciReports)
    {
        var prodRepositoryIds = GetItemIdsFromReport(ciReports, ItemTypes.Repository, projectId);

        var allRepositories = await _azdoClient.GetAsync(Infra.AzdoClient.Requests.Repository.Repositories(
            projectId), organization);

        return allRepositories
            .Select(x => CreateResourceReport(x, organization, projectId, prodRepositoryIds, ciReports));
    }

    private static IEnumerable<string> GetItemIdsFromReport(IEnumerable<CiReport> ciReports, string itemType,
        string projectId) =>
        ciReports
            .SelectMany(ciReport => ciReport.PrincipleReports ?? Array.Empty<PrincipleReport>())
            .SelectMany(principleReport => principleReport.RuleReports ?? Array.Empty<RuleReport>())
            .SelectMany(ruleReport => ruleReport.ItemReports ?? Array.Empty<ItemReport>())
            .Where(itemReport => itemReport.Type == itemType && itemReport.ProjectId == projectId)
            .Select(itemReport => itemReport.ItemId)
            .Distinct();

    private static string GetCiIdentifiers(string itemId, string itemType, IEnumerable<CiReport> ciReports,
        string projectId)
    {
        var ciIdentifiers = ciReports
            .Where(ciReport => ciReport.PrincipleReports != null && ciReport.PrincipleReports
                .SelectMany(principleReport => principleReport.RuleReports ?? Array.Empty<RuleReport>())
                .SelectMany(ruleReport => ruleReport.ItemReports ?? Array.Empty<ItemReport>())
                .Any(itemReport => itemReport.ItemId == itemId && itemReport.Type == itemType &&
                                   itemReport.ProjectId == projectId))
            .Select(c => c.Id);

        return ToCommaSeparatedString(ciIdentifiers);
    }

    private static IEnumerable<(string, string, string)> GetCiInformation(
        IRegisterableDefinition registerableDefinition) =>
        registerableDefinition.PipelineRegistrations.Select(d => (d.CiIdentifier, d.CiName, d.AssignmentGroup))
            .Distinct();

    private static IEnumerable<string> GetRegisteredStageNames(IRegisterableDefinition pipeline)
    {
        var registeredStageIds = pipeline.GetRegisteredStageIds();
        return pipeline.GetStages()
            .Where(stage => registeredStageIds.Contains(stage.Id, StringComparer.OrdinalIgnoreCase))
            .Select(stage => stage.Name);
    }

    private PipelineReport ToPipelineReport(string organization, string projectId,
        ReleaseDefinition releaseDefinition, bool? isProdPipeline) =>
        ToPipelineReport(organization, projectId, releaseDefinition, isProdPipeline,
            Enumerable.Empty<(string, string, string)>(), Enumerable.Empty<string>());

    private PipelineReport ToPipelineReport(string organization, string projectId,
        BuildDefinition yamlPipeline, bool? isProdPipeline) =>
        ToPipelineReport(organization, projectId, yamlPipeline, isProdPipeline,
            Enumerable.Empty<(string, string, string)>(), Enumerable.Empty<string>());

    private PipelineReport ToPipelineReport(string organization, string projectId,
        ReleaseDefinition classicReleasePipeline, bool? isProdPipeline,
        IEnumerable<(string, string, string)> ciInformation, IEnumerable<string> productionStages) =>
        new(classicReleasePipeline.Id, classicReleasePipeline.Name, ItemTypes.ClassicReleasePipeline,
            classicReleasePipeline.Links.Web.Href.AbsoluteUri, isProdPipeline)
        {
            Stages = classicReleasePipeline.GetStages().ToStageReports(),
            RegisterUrl = CreateUrl.RegisterUrl(_config, organization, projectId,
                classicReleasePipeline.Id, ItemTypes.ClassicReleasePipeline),
            OpenPermissionsUrl = CreateUrl.OpenPermissionsUrl(
                _config, organization, projectId, ItemTypes.ReleasePipeline, classicReleasePipeline.Id),
            CiIdentifiers = ToCommaSeparatedString(ciInformation.Select(x => x.Item1).ToList()),
            CiNames = ToCommaSeparatedString(ciInformation.Select(x => x.Item2).ToList()),
            AssignmentGroups = ToCommaSeparatedString(ciInformation.Select(x => x.Item3).Distinct().ToList()),
            ProductionStages = ToCommaSeparatedString(productionStages),
            AddNonProdPipelineToScanUrl = CreateUrl.AddNonProdPipelineToScanUrl(
                _config, organization, projectId, classicReleasePipeline.Id, ItemTypes.ClassicReleasePipeline),
            ExclusionListUrl = CreateUrl.ExclusionListUrl(_config, organization, projectId, classicReleasePipeline.Id,
                ItemTypes.ClassicReleasePipeline),
            UpdateRegistrationUrl = CreateUrl.UpdateRegistrationUrl(_config, organization, projectId,
                classicReleasePipeline.Id, ItemTypes.ClassicReleasePipeline),
            DeleteRegistrationUrl = CreateUrl.DeleteRegistrationUrl(_config, organization, projectId,
                classicReleasePipeline.Id, ItemTypes.ClassicReleasePipeline),
            RuleProfileName = classicReleasePipeline.PipelineRegistrations.FirstOrDefault()?.RuleProfileName,
            Registrations = GetPipelineRegistrationReports(classicReleasePipeline)
        };

    private PipelineReport ToPipelineReport(string organization, string projectId,
        BuildDefinition yamlReleasePipeline, bool? isProdPipeline,
        IEnumerable<(string, string, string)> ciInformation, IEnumerable<string> productionStages) =>
        new(yamlReleasePipeline.Id, yamlReleasePipeline.Name, ItemTypes.YamlReleasePipeline,
            yamlReleasePipeline.Links.Web.Href.AbsoluteUri, isProdPipeline)
        {
            Stages = yamlReleasePipeline.GetStages().ToStageReports(),
            RegisterUrl = CreateUrl.RegisterUrl(_config, organization, projectId,
                yamlReleasePipeline.Id, ItemTypes.YamlReleasePipeline),
            OpenPermissionsUrl = CreateUrl.OpenPermissionsUrl(
                _config, organization, projectId, ItemTypes.BuildPipeline, yamlReleasePipeline.Id),
            CiIdentifiers = ToCommaSeparatedString(ciInformation.Select(x => x.Item1).ToList()),
            CiNames = ToCommaSeparatedString(ciInformation.Select(x => x.Item2).ToList()),
            AssignmentGroups = ToCommaSeparatedString(ciInformation.Select(x => x.Item3).ToList()),
            ProductionStages = ToCommaSeparatedString(productionStages),
            AddNonProdPipelineToScanUrl = CreateUrl.AddNonProdPipelineToScanUrl(
                _config, organization, projectId, yamlReleasePipeline.Id, ItemTypes.YamlReleasePipeline),
            ExclusionListUrl = CreateUrl.ExclusionListUrl(_config, organization, projectId, yamlReleasePipeline.Id,
                ItemTypes.YamlReleasePipeline),
            UpdateRegistrationUrl = CreateUrl.UpdateRegistrationUrl(_config, organization, projectId,
                yamlReleasePipeline.Id, ItemTypes.YamlReleasePipeline),
            DeleteRegistrationUrl = CreateUrl.DeleteRegistrationUrl(_config, organization, projectId,
                yamlReleasePipeline.Id, ItemTypes.YamlReleasePipeline),
            RuleProfileName = yamlReleasePipeline.PipelineRegistrations.FirstOrDefault()?.RuleProfileName,
            Registrations = GetPipelineRegistrationReports(yamlReleasePipeline)
        };

    private static IEnumerable<RegistrationPipelineReport> GetPipelineRegistrationReports(IRegisterableDefinition definition)
    {
        var registrationsForCurrentProject = definition.PipelineRegistrations.Where(pipelineRegistration => pipelineRegistration.IsProduction);

        return registrationsForCurrentProject.Select(pipelineRegistration =>
            new RegistrationPipelineReport(pipelineRegistration.CiIdentifier, pipelineRegistration.CiName,
                pipelineRegistration.StageId));
    }

    private static string ToCommaSeparatedString(IEnumerable<string> input) => string.Join(", ", input);
}