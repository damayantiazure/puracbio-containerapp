#nullable enable

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Rabobank.Compliancy.Core.Approvals.Model;

public class PullRequestApproveLogData
{
    [Display(Name = "CreatedBy_s")]
    public string? CreatedBy { get; set; }

    [Display(Name = "ClosedBy_s")]
    public string? ClosedBy { get; set; }

    [Display(Name = "LastMergeCommitId_s")]
    public string? LastMergeCommitId { get; set; }

    [Display(Name = "Approvers_s")]
    public IEnumerable<string>? Approvers { get; set; }

    [Display(Name = "Status_s")]
    public string? Status { get; set; }
}