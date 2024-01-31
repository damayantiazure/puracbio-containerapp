#nullable enable

using AutoMapper;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Deviations;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Infrastructure.Config;
using Rabobank.Compliancy.Infrastructure.Constants;
using Rabobank.Compliancy.Infrastructure.Dto.CompliancyReport;
using Rabobank.Compliancy.Infrastructure.Extensions;
using Rabobank.Compliancy.Infrastructure.Helpers;

namespace Rabobank.Compliancy.Infrastructure;

public class CompliancyReportService : ICompliancyReportService
{
    private const string _unableToDownloadCompliancyReportError =
        "Unable to download CompliancyReport from ExtensionData.";

    private readonly AzureDevOpsExtensionConfig _config;
    private readonly IDeviationService _deviationService;
    private readonly IExtensionDataRepository _extensionDataRepository;
    private readonly IMapper _mapper;

    public CompliancyReportService(
        IExtensionDataRepository extensionDataRepository,
        AzureDevOpsExtensionConfig config,
        IMapper mapper,
        IDeviationService deviationService)
    {
        _extensionDataRepository = extensionDataRepository;
        _config = config;
        _mapper = mapper;
        _deviationService = deviationService;
    }

    /// <inheritdoc />
    public Task UpdateComplianceReportAsync(string organization, Guid projectId,
        CompliancyReport compliancyReport, DateTime scanDate) =>
        RetryHelper.ExecuteBadRequestPolicyAsync(async () =>
        {
            await AddDeviationsAsync(compliancyReport, projectId, scanDate);

            await UploadCompliancyReportAsync(organization, compliancyReport);
        });

    /// <inheritdoc />
    public Task UpdateCiReportAsync(string organization, Guid projectId, string projectName, CiReport newCiReport,
        DateTime scanDate) =>
        RetryHelper.ExecuteBadRequestPolicyAsync(async () =>
        {
            var compliancyReport = await DownloadCompliancyReportAsync(organization, projectName)
                                   ?? throw new InvalidOperationException(_unableToDownloadCompliancyReportError);

            var oldCiReport =
                compliancyReport.RegisteredConfigurationItems?.SingleOrDefault(r => r.Id == newCiReport.Id)
                ?? throw new InvalidOperationException(
                    $"Unable to find RegisteredConfigurationItem with id: {newCiReport.Id}");

            compliancyReport.RegisteredConfigurationItems!.Replace(oldCiReport, newCiReport);

            await AddDeviationsAsync(compliancyReport, projectId, scanDate);

            await UploadCompliancyReportAsync(organization, compliancyReport);
        });

    /// <inheritdoc />
    public Task UpdateNonProdPipelineReportAsync(string organization, string projectName,
        NonProdCompliancyReport nonProdCompliancyReport) =>
        RetryHelper.ExecuteBadRequestPolicyAsync(async () =>
        {
            var compliancyReport = await DownloadCompliancyReportAsync(organization, projectName)
                                   ?? throw new InvalidOperationException(_unableToDownloadCompliancyReportError);

            var oldProdCompliancyReport =
                compliancyReport.NonProdPipelinesRegisteredForScan?.SingleOrDefault(r =>
                    r.PipelineId == nonProdCompliancyReport.PipelineId)
                ?? throw new InvalidOperationException(
                    $"Unable to find NonProdPipelineRegisteredForScan with id: {nonProdCompliancyReport.PipelineId}");

            compliancyReport.NonProdPipelinesRegisteredForScan!.Replace(oldProdCompliancyReport,
                nonProdCompliancyReport);

            await UploadCompliancyReportAsync(organization, compliancyReport);
        });

    /// <inheritdoc />
    public Task UpdateComplianceStatusAsync(Project project, Guid itemProjectId, string itemId,
        string ruleName, bool isProjectRule, bool isCompliant) =>
        RetryHelper.ExecuteBadRequestPolicyAsync(async () =>
        {
            var compliancyReport = await DownloadCompliancyReportAsync(project.Organization, project.Name)
                                   ?? throw new InvalidOperationException(_unableToDownloadCompliancyReportError);

            if (isProjectRule)
            {
                UpdateComplianceStatusForProjectScope(
                    compliancyReport, ruleName, isCompliant);
            }
            else
            {
                UpdateComplianceStatusNonItemScope(
                    compliancyReport, ruleName, isCompliant, itemProjectId.ToString(), itemId);
            }

            await UploadCompliancyReportAsync(project.Organization, compliancyReport);
        });

    /// <inheritdoc />
    public Task AddDeviationToReportAsync(Deviation? deviation)
    {
        if (deviation == null)
        {
            throw new ArgumentNullException(nameof(deviation));
        }

        return RetryHelper.ExecuteBadRequestPolicyAsync(async () =>
        {
            var compliancyReport =
                await DownloadCompliancyReportAsync(deviation.Project.Organization, deviation.Project.Name)
                ?? throw new InvalidOperationException(_unableToDownloadCompliancyReportError);

            var scanDate = DateTime.UtcNow;

            AddDeviation(compliancyReport, deviation, scanDate);

            await UploadCompliancyReportAsync(deviation.Project.Organization, compliancyReport);
        });
    }

    /// <inheritdoc />
    public Task RemoveDeviationFromReportAsync(Deviation deviation) =>
        RetryHelper.ExecuteBadRequestPolicyAsync(async () =>
        {
            var compliancyReport =
                await DownloadCompliancyReportAsync(deviation.Project.Organization, deviation.Project.Name)
                ?? throw new InvalidOperationException(_unableToDownloadCompliancyReportError);

            var scanDate = DateTime.UtcNow;

            compliancyReport.RemoveDeviation(deviation, scanDate);

            await UploadCompliancyReportAsync(deviation.Project.Organization, compliancyReport);
        });

    /// <inheritdoc />
    public async Task UpdateRegistrationAsync(
        string organization, string projectName, string pipelineId, string pipelineType, string? ciIdentifier) =>
        await RetryHelper.ExecuteBadRequestPolicyAsync(async () =>
        {
            var compliancyReport = await DownloadCompliancyReportAsync(organization, projectName)
                                   ?? throw new InvalidOperationException(_unableToDownloadCompliancyReportError);

            var pipelineReport = compliancyReport.GetPipelineReport(pipelineId, pipelineType)
                                 ?? throw new InvalidOperationException(
                                     $"{pipelineType} pipeline with id: '{pipelineId}' could not be found.");

            compliancyReport.RegisteredPipelines?.Remove(pipelineReport);
            compliancyReport.UnregisteredPipelines?.Remove(pipelineReport);
            compliancyReport.RegisteredPipelinesNoProdStage?.Remove(pipelineReport);

            if (ciIdentifier != null)
            {
                pipelineReport.CiIdentifiers = string.IsNullOrWhiteSpace(pipelineReport.CiIdentifiers)
                    ? ciIdentifier
                    : $"{pipelineReport.CiIdentifiers},{ciIdentifier}";
            }

            pipelineReport.IsProduction = !string.IsNullOrEmpty(ciIdentifier);

            compliancyReport.RegisteredPipelines = compliancyReport.RegisteredPipelines == null
                ? new[] { pipelineReport }
                : compliancyReport.RegisteredPipelines
                    .Concat(new[] { pipelineReport })
                    .OrderBy(p => p.Name)
                    .ToList();

            await UploadCompliancyReportAsync(organization, compliancyReport);
        });

    /// <inheritdoc/>
    public async Task UnRegisteredPipelineAsync(string organization, string projectName, string pipelineId, string pipelineType, CancellationToken cancellationToken = default)
    {
        await RetryHelper.ExecuteBadRequestPolicyAsync(async () =>
        {
            var compliancyReport = await DownloadCompliancyReportAsync(organization, projectName)
                ?? throw new InvalidOperationException(_unableToDownloadCompliancyReportError);

            var pipelineReport = compliancyReport.RegisteredPipelines?.SingleOrDefault(pipelineReport =>
                   pipelineReport.Id == pipelineId && pipelineReport.Type == pipelineType) ??
                    compliancyReport.RegisteredPipelinesNoProdStage?.SingleOrDefault(pipelineReport =>
                        pipelineReport.Id == pipelineId && pipelineReport.Type == pipelineType)
                ?? throw new InvalidOperationException($"{pipelineType} pipeline with id: '{pipelineId}' could not be found.");

            compliancyReport.RegisteredPipelines?.Remove(pipelineReport);
            compliancyReport.RegisteredPipelinesNoProdStage?.Remove(pipelineReport);

            compliancyReport.UnregisteredPipelines?.Add(pipelineReport);

            await UploadCompliancyReportAsync(organization, compliancyReport, cancellationToken);
        });
    }

    private static void UpdateComplianceStatusForProjectScope(CompliancyReport compliancyReport, string ruleName,
        bool isCompliant)
    {
        var scanDate = DateTime.UtcNow;

        var ruleReports = compliancyReport.GetRuleReports(ruleName);
        var ruleReportsNonProd = compliancyReport.GetNonProdPipelineRuleReports(ruleName);

        var itemReports = ruleReports.SelectMany(r => r.ItemReports ?? Array.Empty<ItemReport>());
        var itemReportsNonProd = ruleReportsNonProd.SelectMany(r => r.ItemReports ?? Array.Empty<ItemReport>());

        var allItemReports = itemReports.Concat(itemReportsNonProd);

        allItemReports.UpdateComplianceStatusForItemReports(isCompliant, scanDate);
    }

    private static void UpdateComplianceStatusNonItemScope(CompliancyReport compliancyReport, string ruleName,
        bool isCompliant, string itemProjectId, string itemId)
    {
        var scanDate = DateTime.UtcNow;

        var itemReports = compliancyReport.GetItemReports(itemProjectId, itemId, ruleName);
        var itemReportsNonProd = compliancyReport.GetNonProdPipelineItemReports(itemProjectId, itemId, ruleName);

        var allItemReports = itemReports.Concat(itemReportsNonProd);

        allItemReports.UpdateComplianceStatusForItemReports(isCompliant, scanDate);
    }

    private async Task AddDeviationsAsync(CompliancyReport compliancyReport, Guid projectId, DateTime scanDate)
    {
        var deviations = await _deviationService.GetDeviationsAsync(projectId);

        deviations.ToList().ForEach(deviation => AddDeviation(compliancyReport, deviation, scanDate));
    }

    private void AddDeviation(CompliancyReport compliancyReport, Deviation deviation, DateTime scanDate)
    {
        var foreignProjectId = deviation.ItemProjectId?.ToString();
        var projectId = deviation.Project.Id.ToString();

        var itemProjectId = foreignProjectId ?? projectId;

        var itemReports = compliancyReport.GetItemReportsByCiIdentifier(
            itemProjectId, deviation.ItemId, deviation.RuleName, deviation.CiIdentifier);

        var deviationReport = _mapper.Map<DeviationReport>(deviation);

        itemReports.ToList().ForEach(i =>
        {
            i.Deviation = deviationReport;
            i.ScanDate = scanDate;
        });
    }

    private async Task<CompliancyReport?> DownloadCompliancyReportAsync(string organization, string projectName)
    {
        var compliancyReportDto = await _extensionDataRepository.DownloadAsync<CompliancyReportDto>(
            CompliancyScannerExtensionConstants.Publisher,
            CompliancyScannerExtensionConstants.Collection, _config.ExtensionName, organization, projectName);

        return _mapper.Map<CompliancyReport>(compliancyReportDto);
    }

    private Task UploadCompliancyReportAsync(string organization, CompliancyReport compliancyReport, CancellationToken cancellationToken = default)
    {
        var compliancyReportDto = _mapper.Map<CompliancyReportDto>(compliancyReport);

        return _extensionDataRepository.UploadAsync(CompliancyScannerExtensionConstants.Publisher,
            CompliancyScannerExtensionConstants.Collection, _config.ExtensionName, organization, compliancyReportDto, cancellationToken);
    }
}