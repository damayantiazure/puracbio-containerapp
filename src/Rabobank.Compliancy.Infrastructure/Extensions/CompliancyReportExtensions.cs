#nullable enable

using Microsoft.VisualStudio.Services.Common;
using Rabobank.Compliancy.Domain.Compliancy.Deviations;
using Rabobank.Compliancy.Domain.Compliancy.Reports;

namespace Rabobank.Compliancy.Infrastructure.Extensions;

internal static class CompliancyReportExtensions
{
    internal static IEnumerable<RuleReport> GetRuleReports(this CompliancyReport compliancyReport, string ruleName) =>
        compliancyReport.RegisteredConfigurationItems == null
            ? Array.Empty<RuleReport>()
            : compliancyReport.RegisteredConfigurationItems
                .SelectMany(ciReport => ciReport.PrincipleReports ?? Array.Empty<PrincipleReport>())
                .SelectMany(principleReport => principleReport.RuleReports ?? Array.Empty<RuleReport>())
                .Where(ruleReport => ruleReport.Name == ruleName);

    internal static IEnumerable<RuleReport>
        GetNonProdPipelineRuleReports(this CompliancyReport compliancyReport, string ruleName) =>
        compliancyReport.NonProdPipelinesRegisteredForScan == null
            ? Array.Empty<RuleReport>()
            : compliancyReport.NonProdPipelinesRegisteredForScan
                .SelectMany(nonProdCompliancyReport =>
                    nonProdCompliancyReport.PrincipleReports ?? Array.Empty<PrincipleReport>())
                .SelectMany(principleReport => principleReport.RuleReports ?? Array.Empty<RuleReport>())
                .Where(ruleReport => ruleReport.Name == ruleName);

    internal static IEnumerable<ItemReport> GetItemReports(this CompliancyReport compliancyReport,
        string itemProjectId, string itemId, string ruleName) =>
        compliancyReport.RegisteredConfigurationItems == null
            ? Array.Empty<ItemReport>()
            : compliancyReport.RegisteredConfigurationItems
                .SelectMany(ciReport => ciReport.PrincipleReports ?? Array.Empty<PrincipleReport>())
                .SelectMany(principleReport => principleReport.RuleReports ?? Array.Empty<RuleReport>())
                .Where(ruleReport => ruleReport.Name == ruleName)
                .SelectMany(ruleReport => ruleReport.ItemReports ?? Array.Empty<ItemReport>())
                .Where(itemReport => itemReport.ItemId == itemId && itemReport.ProjectId == itemProjectId);

    internal static IEnumerable<ItemReport> GetNonProdPipelineItemReports(
        this CompliancyReport compliancyReport,
        string itemProjectId, string itemId, string ruleName) =>
        compliancyReport.NonProdPipelinesRegisteredForScan == null
            ? Array.Empty<ItemReport>()
            : compliancyReport.NonProdPipelinesRegisteredForScan
                .SelectMany(nonProdCompliancyReport =>
                    nonProdCompliancyReport.PrincipleReports ?? Array.Empty<PrincipleReport>())
                .SelectMany(principleReport => principleReport.RuleReports ?? Array.Empty<RuleReport>())
                .Where(ruleReport => ruleReport.Name == ruleName)
                .SelectMany(ruleReport => ruleReport.ItemReports ?? Array.Empty<ItemReport>())
                .Where(itemReport => itemReport.ItemId == itemId && itemReport.ProjectId == itemProjectId);

    internal static void UpdateComplianceStatusForItemReports(this IEnumerable<ItemReport> itemReports,
        bool isCompliant,
        DateTime scanDate) => itemReports
        .ForEach(itemReport =>
        {
            itemReport.IsCompliantForRule = isCompliant;
            itemReport.ScanDate = scanDate;
        });

    internal static CompliancyReport RemoveDeviation(this CompliancyReport compliancyReport,
        Deviation deviation, DateTime scanDate)
    {
        var foreignProjectId = deviation.ItemProjectId?.ToString();
        var projectId = deviation.Project.Id.ToString();

        var itemProjectId = foreignProjectId ?? projectId;

        var itemReports = compliancyReport.GetItemReportsByCiIdentifier(itemProjectId, deviation.ItemId,
            deviation.RuleName,
            deviation.CiIdentifier);

        itemReports
            .ForEach(itemReport =>
            {
                itemReport.Deviation = null;
                itemReport.ScanDate = scanDate;
            });

        return compliancyReport;
    }

    internal static IEnumerable<ItemReport> GetItemReportsByCiIdentifier(
        this CompliancyReport compliancyReport,
        string itemProjectId, string itemId, string ruleName, string ciIdentifier) =>
        compliancyReport.RegisteredConfigurationItems == null
            ? Array.Empty<ItemReport>()
            : compliancyReport.RegisteredConfigurationItems
                .Where(ciReport => ciReport.Id == ciIdentifier)
                .SelectMany(ciReport => ciReport.PrincipleReports ?? Array.Empty<PrincipleReport>())
                .SelectMany(principleReport => principleReport.RuleReports ?? Array.Empty<RuleReport>())
                .Where(ruleReport => ruleReport.Name == ruleName)
                .SelectMany(ruleReport => ruleReport.ItemReports ?? Array.Empty<ItemReport>())
                .Where(itemReport => itemReport.ItemId == itemId && itemReport.ProjectId == itemProjectId);

    internal static PipelineReport? GetPipelineReport(this CompliancyReport compliancyReportDto,
        string pipelineId, string pipelineType) =>
        compliancyReportDto.UnregisteredPipelines?.SingleOrDefault(pipelineReport =>
            pipelineReport.Id == pipelineId && pipelineReport.Type == pipelineType)
        ?? compliancyReportDto.RegisteredPipelines?.SingleOrDefault(pipelineReport =>
            pipelineReport.Id == pipelineId && pipelineReport.Type == pipelineType)
        ?? compliancyReportDto.RegisteredPipelinesNoProdStage?.SingleOrDefault(pipelineReport =>
            pipelineReport.Id == pipelineId && pipelineReport.Type == pipelineType);
}