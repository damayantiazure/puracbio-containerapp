#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Domain.Compliancy.Reports;

[ExcludeFromCodeCoverage]
public class CiReport
{
    public CiReport(string id, string? name, DateTime scanDate)
    {
        Id = id;
        Name = name;
        ScanDate = scanDate;
    }

    [Display(Name = "ciId_s")]
    public string Id { get; }

    [Display(Name = "ciName_s")]
    public string? Name { get; }

    [Display(Name = "scanDate_t")]
    public DateTime ScanDate { get; }

    [Display(Name = "ciAICRating_s")]
    public string? AicRating { get; set; }

    [Display(Name = "ciSubtype_s")]
    public string? CiSubtype { get; set; }

    [Display(Name = "assignmentGroup_s")]
    public string? AssignmentGroup { get; set; }

    [Display(Name = "isSOx_b")]
    public bool IsSOx { get; set; }

    [Display(Name = "isCompliant_b")]
    public bool IsCompliant =>
        PrincipleReports != null &&
        PrincipleReports.Any() &&
        PrincipleReports.All(r => r.IsCompliant);

    [Display(Name = "isSOxCompliant_b")]
    public bool IsSOxCompliant =>
        PrincipleReports != null &&
        PrincipleReports.Any() &&
        PrincipleReports
            .Where(x => x.IsSox)
            .All(x => x.IsCompliant);

    public IEnumerable<PrincipleReport>? PrincipleReports { get; set; }

    public Uri? RescanUrl { get; set; }

    public bool IsScanFailed { get; set; }

    public ExceptionSummaryReport? ScanException { get; set; }

    [Display(Name = "hasDeviation_b")]
    public bool HasDeviation =>
        PrincipleReports != null &&
        PrincipleReports.Any(r => r.HasDeviation);

    [Display(Name = "organization_s")]
    public string? Organization { get; set; }

    [Display(Name = "projectId_g")]
    public string? ProjectId { get; set; }

    [Display(Name = "projectName_s")]
    public string? ProjectName { get; set; }
}