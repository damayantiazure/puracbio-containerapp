#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Domain.Compliancy.Reports;

[ExcludeFromCodeCoverage]
public class ItemReport
{
    public ItemReport(string itemId, string name, string projectId, DateTime scanDate)
    {
        ItemId = itemId;
        Name = name;
        ProjectId = projectId;
        ScanDate = scanDate;
    }

    [Display(Name = "itemId_s")]
    public string ItemId { get; }

    [Display(Name = "itemName_s")]
    public string Name { get; }

    [Display(Name = "projectId_g")]
    public string ProjectId { get; }

    [Display(Name = "scanDate_t")]
    public DateTime ScanDate { get; set; }

    public string? Type { get; init; }

    public string? Link { get; init; }

    [Display(Name = "isCompliant_b")]
    public bool IsCompliant => HasDeviation || IsCompliantForRule;

    public bool IsCompliantForRule { get; set; }

    public Uri? ReconcileUrl { get; init; }

    public Uri? RescanUrl { get; init; }

    public Uri? RegisterDeviationUrl { get; init; }

    public Uri? DeleteDeviationUrl { get; init; }

    public string[]? ReconcileImpact { get; init; }

    public DeviationReport? Deviation { get; set; }

    [Display(Name = "hasDeviation_b")]
    public bool HasDeviation => Deviation != null;

    [Display(Name = "organization_s")]
    public string? Organization { get; init; }

    [Display(Name = "projectName_s")]
    public string? ProjectName { get; init; }

    [Display(Name = "ciId_s")]
    public string? CiId { get; init; }

    [Display(Name = "ciName_s")]
    public string? CiName { get; init; }

    [Display(Name = "principleName_s")]
    public string? PrincipleName { get; init; }

    [Display(Name = "ruleName_s")]
    public string? RuleName { get; init; }
}