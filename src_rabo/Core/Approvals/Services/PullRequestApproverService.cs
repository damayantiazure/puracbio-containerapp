#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Flurl.Http;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.Approvals.Model;
using Rabobank.Compliancy.Core.Approvals.Utils;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Repository = Rabobank.Compliancy.Infra.AzdoClient.Requests.Repository;

namespace Rabobank.Compliancy.Core.Approvals.Services;

public class PullRequestApproverService : IPullRequestApproverService
{
    private const string _repositoryRegex = @"repositories\/(.*)\/commits";
    private const int _approvedVote = 10;
    private const int _approvedWithSuggestionsVote = 5;
    private const string _completedStatus = "completed";
    private const int _pullRequestsToRetrieve = 100;
    private const string _git = "TfsGit";
    private readonly IAzdoRestClient _azdoClient;
    private readonly ILogQueryService _logQueryService;

    public PullRequestApproverService(IAzdoRestClient azdoClient, ILogQueryService logQueryService)
    {
        _azdoClient = azdoClient;
        _logQueryService = logQueryService;
    }

    public async Task<bool> HasApprovalAsync(string projectId, string runId, string? organization = null)
    {
        var (repositoryId, commitId) = await GetLastCommitDetailsAsync(projectId, runId, organization);
        var approvalData = await GetApprovalDataFromLog(commitId);

        // Fallback
        if (approvalData?.ClosedBy == null)
        {
            var pullRequest = await GetPullRequestAsync(repositoryId, commitId, organization);
            var approvers = await GetApproversFromPrAsync(pullRequest, organization);
            string? exclude = null;

            if (pullRequest != null)
            {
                exclude = pullRequest.ClosedBy?.Id == null
                    ? pullRequest.CreatedBy.Id.ToString()
                    : pullRequest.ClosedBy.Id.ToString();
            }

            return approvers.Any(r => IsValidApprover(r, exclude) && IsValidEmail(r.UniqueName));
        }
        // Use data written to Log Analytics by hooks
        else
        {
            IEnumerable<string> approvers;

            if (approvalData.Approvers != null && approvalData.Approvers.Any())
            {
                approvers = approvalData.Approvers.Where(IsValidEmail);
            }
            // Approvers that have approved with the vote 'ApprovedWithSuggestions' are not written to LogAnalytics
            // so fetch the approvers from the pull-request.
            else
            {
                var pullRequest = await GetPullRequestAsync(repositoryId, commitId, organization);
                approvers = (await GetApproversFromPrAsync(pullRequest, organization)).Select(a => a.UniqueName);
            }

            return approvers.Any(a => IsValidApprover(a, approvalData.ClosedBy));
        }
    }

    public async Task<IEnumerable<string>> GetAllApproversAsync(string projectId, string runId,
        string? organization = null)
    {
        var (repositoryId, commitId) = await GetLastCommitDetailsAsync(projectId, runId, organization);
        var approvalData = await GetApprovalDataFromLog(commitId);

        if (approvalData?.ClosedBy != null)
        {
            // Use data written to Log Analytics by hooks
            return approvalData.Approvers == null
                ? Enumerable.Empty<string>()
                : approvalData.Approvers.Where(IsValidEmail);
        }

        // Fallback
        var pullRequest = await GetPullRequestAsync(repositoryId, commitId, organization);
        var approvers = await GetApproversAsync(projectId, pullRequest, organization);

        return approvers
            .Where(IsValidApprover)
            .Select(a => a.UniqueName)
            .Distinct()
            .Where(IsValidEmail);
    }

    private async Task<IEnumerable<IdentityRefWithVote>> GetApproversFromPrAsync(GitPullRequest? pullRequest,
        string? organization)
    {
        var repoProjectId = pullRequest?.Repository.Project.Id;
        return await GetApproversAsync(repoProjectId, pullRequest, organization);
    }

    private async Task<PullRequestApproveLogData?> GetApprovalDataFromLog(string? commitId)
    {
        if (commitId == default)
        {
            return default;
        }

        var query =
            $"audit_pull_request_approvers_log_CL | project Approvers_s, CreatedBy_s, ClosedBy_s, LastMergeCommitId_s, Status_s " +
            $"| where LastMergeCommitId_s == \"{commitId}\" and Status_s == \"completed\"";
        try
        {
            return await _logQueryService.GetQueryEntryAsync<PullRequestApproveLogData>(query);
        }
        // Whatever the reason is that the query to LogAnalytics fails, we always want to use the fallback
        catch (Exception)
        {
            return default;
        }
    }

    private async Task<GitPullRequest?> GetPullRequestAsync(string? repositoryId, string? commitId,
        string? organization)
    {
        if (repositoryId == default)
        {
            return default;
        }

        var pullRequests = await _azdoClient.GetAsync(Repository.PullRequests(
            repositoryId, _completedStatus, _pullRequestsToRetrieve), organization);
        var pullRequest = pullRequests.FirstOrDefault(p => p.LastMergeCommit?.CommitId == commitId);

        if (pullRequest == default)
        {
            return default;
        }

        return await _azdoClient.GetAsync(Repository.PullRequest(
            repositoryId, pullRequest.PullRequestId), organization);
    }

    private async Task<(string?, string?)> GetLastCommitDetailsAsync(string projectId, string runId,
        string? organization)
    {
        var commits = new List<Change>();
        try
        {
            commits = (await _azdoClient.GetAsync(Builds.Changes(projectId, runId), organization)).ToList();
        }
        // Do not throw for Internal Server Error that occurs when a repository has been renamed or removed
        catch (FlurlHttpException e)
        {
            if (e.Call?.HttpStatus != HttpStatusCode.InternalServerError)
            {
                throw;
            }
        }

        var lastCommit = commits.FirstOrDefault();
        if (lastCommit is { Type: _git })
        {
            var repositoryId = Regex.Match(lastCommit.Location, _repositoryRegex).Groups[1].Value;
            return (repositoryId, lastCommit.Id);
        }

        var build = await _azdoClient.GetAsync(Builds.Build(projectId, runId), organization);
        return build?.Repository?.Type == _git ? (build.Repository.Id, build.SourceVersion) : (default, default);
    }

    private async Task<IEnumerable<IdentityRefWithVote>> GetApproversAsync(
        string? projectId, GitPullRequest? pullRequest, string? organization)
    {
        if (pullRequest == default)
        {
            return Enumerable.Empty<IdentityRefWithVote>();
        }

        return await _azdoClient.GetAsync(Repository.PullRequestReviewers(
            projectId, pullRequest.Repository.Id, pullRequest.PullRequestId.ToString()), organization);
    }

    private static bool IsValidApprover(string approver, string exclude) =>
        !approver.Equals(exclude, StringComparison.InvariantCultureIgnoreCase);

    private static bool IsValidApprover(IdentityRefWithVote approver, string? exclude)
    {
        int[] validApprovers = { _approvedVote, _approvedWithSuggestionsVote };
        return !approver.IsContainer && validApprovers.Contains(approver.Vote) && approver.Id != exclude;
    }

    private static bool IsValidApprover(IdentityRefWithVote approver) =>
        IsValidApprover(approver, null);

    private static bool IsValidEmail(string? email) =>
        MailChecker.IsValidEmail(email);
}