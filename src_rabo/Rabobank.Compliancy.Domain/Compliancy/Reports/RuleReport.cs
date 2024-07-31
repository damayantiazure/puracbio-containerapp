#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Domain.Compliancy.Reports;

[ExcludeFromCodeCoverage]
public class RuleReport
{
    public RuleReport(string name, DateTime scanDate)
    {
        Name = name;
        ScanDate = scanDate;
    }

    [Display(Name = "ruleName_s")]
    public string Name { get; }

    [Display(Name = "scanDate_t")]
    public DateTime ScanDate { get; }

    public string? Description { get; init; }

    [Display(Name = "isCompliant_b")]
    public bool IsCompliant =>
        ItemReports != null &&
        ItemReports.Any() &&
        ItemReports.All(r => r.IsCompliant);

    public Uri? DocumentationUrl { get; init; }

    public IEnumerable<ItemReport>? ItemReports { get; init; }

    [Display(Name = "hasDeviation_b")]
    public bool HasDeviation =>
        ItemReports != null &&
        ItemReports.Any(r => r.HasDeviation);

    [Display(Name = "organization_s")]
    public string? Organization { get; init; }

    [Display(Name = "projectId_g")]
    public string? ProjectId { get; init; }

    [Display(Name = "projectName_s")]
    public string? ProjectName { get; init; }

    [Display(Name = "ciId_s")]
    public string? CiId { get; init; }

    [Display(Name = "ciName_s")]
    public string? CiName { get; init; }

    [Display(Name = "principleName_s")]
    public string? PrincipleName { get; init; }

    [Display(Name = "ruleDocumentation_s")]
    public string? RuleDocumentation { get; init; }
}