#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infrastructure.Dto.Logging;

[ExcludeFromCodeCoverage]
public class AuditPullRequestApproversLogDto : LogModelDtoBase
{
    [Display(Name = "CreatedBy_s")]
    public string? CreatedBy { get; set; }

    [Display(Name = "ClosedBy_s")]
    public string? ClosedBy { get; set; }

    [Display(Name = "Organization_s")]
    public string? Organization { get; set; }

    [Display(Name = "ProjectId_g")]
    public string? ProjectId { get; set; }

    [Display(Name = "ProjectName_s")]
    public string? ProjectName { get; set; }

    [Display(Name = "PullRequestId_s")]
    public string? PullRequestId { get; set; }

    [Display(Name = "PullRequestUrl_s")]
    public string? PullRequestUrl { get; set; }

    [Display(Name = "RepositoryId_g")]
    public string? RepositoryId { get; set; }

    [Display(Name = "RepositoryUrl_s")]
    public string? RepositoryUrl { get; set; }

    [Display(Name = "Status_s")]
    public string? Status { get; set; }

    [Display(Name = "CreationDate_t")]
    public DateTime CreationDate { get; set; }

    [Display(Name = "ClosedDate_t")]
    public DateTime ClosedDate { get; set; }

    [Display(Name = "Approvers_s")]
    public IEnumerable<string>? Approvers { get; set; }

    [Display(Name = "LastMergeCommitId_s")]
    public string? LastMergeCommitId { get; set; }

    [Display(Name = "LastMergeSourceCommit_s")]
    public string? LastMergeSourceCommit { get; set; }

    [Display(Name = "LastMergeTargetCommit_s")]
    public string? LastMergeTargetCommit { get; set; }
}