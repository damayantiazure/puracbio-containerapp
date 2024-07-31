#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Domain.Compliancy.Reports;

[ExcludeFromCodeCoverage]
public class DeviationReport
{
    [Display(Name = "deviation_Comment_s")]
    public string? Comment { get; set; }

    [Display(Name = "deviation_Reason_s")]
    public string? Reason { get; set; }

    [Display(Name = "deviation_ReasonNotApplicable_s")]
    public string? ReasonNotApplicable { get; set; }

    [Display(Name = "deviation_ReasonNotApplicableOther_s")]
    public string? ReasonNotApplicableOther { get; set; }

    [Display(Name = "deviation_ReasonOther_s")]
    public string? ReasonOther { get; set; }

    [Display(Name = "deviation_UpdatedBy_s")]
    public string? UpdatedBy { get; set; }
}