#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infrastructure.Dto.Logging;

[ExcludeFromCodeCoverage]
public class CompliancyCisDto: LogModelDtoBase
{
    [Display(Name = "ciAICRating_s")]
    public string? AicRating { get; set; } 

    [Display(Name = "ciSubtype_s")]
    public string? CiSubtype { get; set; } 

    [Display(Name = "assignmentGroup_s")]
    public string? AssignmentGroup { get; set; } 

    [Display(Name = "organization_s")]
    public string? Organization { get; set; } 

    [Display(Name = "hasDeviation_b")]
    public bool HasDeviation { get; set; }

    [Display(Name = "isSOxCompliant_b")]
    public bool IsSOxCompliant { get; set; }

    [Display(Name = "isSOx_b")]
    public bool IsSOx { get; set; }

    [Display(Name = "projectId_g")]
    public string? ProjectId { get; set; }

    [Display(Name = "projectName_s")]
    public string? ProjectName { get; set; } 

    [Display(Name = "ciId_s")]
    public string? Id { get; set; } 

    [Display(Name = "ciName_s")]
    public string? Name { get; set; } 

    [Display(Name = "isCompliant_b")]
    public bool IsCompliant { get; set; }

    [Display(Name = "scanDate_t")]
    public DateTime? ScanDate { get; set; }
}