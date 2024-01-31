#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infrastructure.Dto.Logging;

[ExcludeFromCodeCoverage]
public class CompliancyItemsDto: LogModelDtoBase
{
    [Display(Name = "itemId_s")]
    public string? ItemId { get; set; }

    [Display(Name = "itemName_s")]
    public string? Name { get; set; }

    [Display(Name = "deviation_Comment_s")]
    public string? DeviationComment { get; set; } 

    [Display(Name = "deviation_Reason_s")]
    public string? DeviationReason { get; set; } 

    [Display(Name = "deviation_ReasonNotApplicable_s")]
    public string? DeviationReasonNotApplicable { get; set; } 

    [Display(Name = "deviation_ReasonNotApplicableOther_s")]
    public string? DeviationReasonNotApplicableOther { get; set; } 

    [Display(Name = "deviation_ReasonOther_s")]
    public string? DeviationReasonOther { get; set; } 

    [Display(Name = "deviation_UpdatedBy_s")]
    public string? DeviationUpdatedBy { get; set; } 

    [Display(Name = "projectId_g")]
    public string? ProjectId { get; set; }

    [Display(Name = "projectName_s")]
    public string? ProjectName { get; set; } 

    [Display(Name = "organization_s")]
    public string? Organization { get; set; } 

    [Display(Name = "ciId_s")]
    public string? CiId { get; set; } 

    [Display(Name = "ciName_s")]
    public string? CiName { get; set; } 

    [Display(Name = "principleName_s")]
    public string? PrincipleName { get; set; } 

    [Display(Name = "ruleName_s")]
    public string? RuleName { get; set; } 

    [Display(Name = "scanDate_t")]
    public DateTime? ScanDate { get; set; }

    [Display(Name = "isCompliant_b")]
    public bool IsCompliant { get; set; }

    [Display(Name = "hasDeviation_b")]
    public bool HasDeviation { get; set; }
}