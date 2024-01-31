#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infrastructure.Dto.Logging;

[ExcludeFromCodeCoverage]
public class DeviationsLogDto : LogModelDtoBase
{
    [Display(Name = "ProjectId_g")]
    public Guid? ProjectId { get; set; }

    [Display(Name = "RuleName_s")]
    public string? RuleName { get; set; }

    [Display(Name = "ItemId_s")]
    public string? ItemId { get; set; }

    [Display(Name = "ItemProjectId_g")]
    public Guid? ItemProjectId { get; set; }

    [Display(Name = "CiIdentifier_s")]
    public string? CiIdentifier { get; set; }

    [Display(Name = "Comment_s")]
    public string? Comment { get; set; }

    [Display(Name = "RecordType_s")]
    public string? RecordType { get; set; }

    [Display(Name = "Reason_s")]
    public string? Reason { get; set; }

    [Display(Name = "ReasonOther_s")]
    public string? ReasonOther { get; set; }

    [Display(Name = "ReasonNotApplicableOther_s")]
    public string? ReasonNotApplicableOther { get; set; }

    [Display(Name = "ReasonNotApplicable_s")]
    public string? ReasonNotApplicable { get; set; }
}