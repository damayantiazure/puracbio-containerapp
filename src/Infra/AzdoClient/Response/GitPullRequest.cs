using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class GitPullRequest
{
    public string ArtifactId { get; set; }
    public IdentityRef AutoCompleteSetBy { get; set; }
    public IdentityRef ClosedBy { get; set; }
    public string ClosedDate { get; set; }
    public int CodeReviewId { get; set; }
    public IEnumerable<GitCommitRef> Commits { get; set; }
    public IdentityRef CreatedBy { get; set; }
    public string Description { get; set; }
    public GitCommitRef LastMergeCommit { get; set; }
    public int PullRequestId { get; set; }
    public Repository Repository { get; set; }
    public IEnumerable<IdentityRefWithVote> Reviewers { get; set; }
    public string SourceRefName { get; set; }
    public string Status { get; set; }
    public string TargetRefName { get; set; }
    public string Title { get; set; }
}