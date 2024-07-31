using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model.Constants;

namespace Rabobank.Compliancy.Functions.PipelineBreaker.Model;

/// <summary>
///     The decorator verifies what the return message starts with:
///         No prefix -> Task succeeds
///         WARNING prefix -> A warning is thrown
///         ERROR prefix -> Pipeline is blocked
/// </summary>
public static class ComplianceResultMessages
{
    public static string GetResultMessage(PipelineBreakerReport report)
    {
        if (report.IsExcluded)
        {
            return DecoratorResultMessages.ExclusionList;
        }

        switch (report.Result)
        {
            case PipelineBreakerResult.Passed:
                return DecoratorResultMessages.Passed;

            case PipelineBreakerResult.Warned:
                return $"{DecoratorResultMessages.WarningNotCompliant}{Environment.NewLine}{ConstructCompliancyReportMessage(report.RuleCompliancyReports)}";

            case PipelineBreakerResult.Blocked:
                return $"{DecoratorResultMessages.NotCompliant}{Environment.NewLine}{ConstructCompliancyReportMessage(report.RuleCompliancyReports)}";

            default: 
                return string.Empty;
        }
    }

    private static string ConstructCompliancyReportMessage(IEnumerable<RuleCompliancyReport> reports)
    {
        StringBuilder reportString = new ();
        foreach (var report in reports.Where(r => !r.IsDeterminedCompliant()))
        {
            reportString.AppendLine(report.ToString());
        }

        if (!reports.All(r => r.IsDeterminedCompliant()))
        {
            reportString.AppendLine($"For more information on how to become compliant, visit: {ConfluenceLinks.CompliancyDocumentation} ");
        }

        return reportString.ToString();
    }
}