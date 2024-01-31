#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Domain.Compliancy.Reports;

[ExcludeFromCodeCoverage]
public class PrincipleReport
{
    public PrincipleReport(string name, DateTime scanDate)
    {
        Name = name;
        ScanDate = scanDate;
    }

    [Display(Name = "principleName_s")]
    public string Name { get;  }

    [Display(Name = "scanDate_t")]
    public DateTime ScanDate { get; }

    public bool HasRulesToCheck { get; init; }

    public bool IsSox { get; init; }

    [Display(Name = "isCompliant_b")]
    public bool IsCompliant =>
        !HasRulesToCheck ||
        (RuleReports != null && RuleReports.Any() && RuleReports.All(r => r.IsCompliant));

    public IList<RuleReport>? RuleReports { get; init; }

    [Display(Name = "hasDeviation_b")]
    public bool HasDeviation => 
        RuleReports != null && 
        RuleReports.Any(r => r.HasDeviation);

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
}