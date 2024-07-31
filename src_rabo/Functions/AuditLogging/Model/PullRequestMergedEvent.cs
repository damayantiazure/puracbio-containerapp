
using System;
using System.Collections.Generic;

namespace Rabobank.Compliancy.Functions.AuditLogging.Model;

public class PullRequestMergedEvent
{
    public string Organization { get; set; }
    public string ProjectId { get; set; }
    public string ProjectName { get; set; }
    public string PullRequestId { get; set; }
    public string PullRequestUrl { get; set; }
    public string RepositoryId { get; set; }
    public string RepositoryUrl { get; set; }
    public string Status { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime ClosedDate { get; set; }
    public IEnumerable<string> Approvers { get; set; } 
    public string LastMergeCommitId { get; set; }
    public string LastMergeSourceCommit { get; set; }
    public string LastMergeTargetCommit { get; set; }
    public string CreatedBy { get; set; }
    public string ClosedBy { get; set; }
}