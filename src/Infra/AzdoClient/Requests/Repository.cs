using Newtonsoft.Json.Linq;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Requests;

public static class Repository
{
    private const int TIMEOUT = 60;

    public static IAzdoRequest<GitCommitRef> Commit(string project, string repositoryId, string commitId) =>
        new AzdoRequest<GitCommitRef>($"/{project}/_apis/git/repositories/{repositoryId}/commits/{commitId}");

    public static IEnumerableRequest<GitCommitRef> Commits(string project, string repositoryId) =>
        new AzdoRequest<GitCommitRef>($"/{project}/_apis/git/repositories/{repositoryId}/commits").AsEnumerable();

    public static IAzdoRequest<Multiple<GitCommitRef>> Commits(string project, string repositoryId, int top) =>
        new AzdoRequest<Multiple<GitCommitRef>>(
            $"/{project}/_apis/git/repositories/{repositoryId}/commits", new Dictionary<string, object>
            {
                {"top", top},
                {"api-version", "6.1-preview"}
            });

    public static IAzdoRequest<JObject> GitItem(string project, string repositoryId, string path) =>
        new AzdoRequest<JObject>($"/{project}/_apis/git/repositories/{repositoryId}/items",
            new Dictionary<string, object>
            {
                { "path", $"{path}" },
                { "includeContent", true },
                { "$format", "json" },
                { "versionDescriptor.versionType", "branch" },
                { "versionDescriptor.version", "master" }
            });

    public static IAzdoRequest<GitPullRequest> PullRequest(string project, string repositoryId, int pullRequestId) =>
        new AzdoRequest<GitPullRequest>($"/{project}/_apis/git/repositories/{repositoryId}/pullrequests/{pullRequestId}",
            new Dictionary<string, object>
            {
                { "api-version", "6.0" }
            });

    public static IAzdoRequest<GitPullRequest> PullRequest(string repositoryId, int pullRequestId) =>
        new AzdoRequest<GitPullRequest>($"/_apis/git/repositories/{repositoryId}/pullrequests/{pullRequestId}",
            new Dictionary<string, object>
            {
                { "api-version", "6.0" }
            });

    public static IEnumerableRequest<GitCommitRef> PullRequestCommits(string project, string repositoryId, string pullRequestId) =>
        new AzdoRequest<GitCommitRef>($"/{project}/_apis/git/repositories/{repositoryId}/pullrequests/{pullRequestId}/commits").AsEnumerable();

    public static IEnumerableRequest<IdentityRefWithVote> PullRequestReviewers(
        string project, string repositoryId, string pullRequestId) =>
        new AzdoRequest<IdentityRefWithVote>(
            $"/{project}/_apis/git/repositories/{repositoryId}/pullrequests/{pullRequestId}/reviewers").AsEnumerable();

    public static IEnumerableRequest<GitPullRequest> PullRequests(
        string project, string repositoryId, string status, int top) =>
        new AzdoRequest<GitPullRequest>($"/{project}/_apis/git/repositories/{repositoryId}/pullrequests",
            new Dictionary<string, object>
            {
                { "searchCriteria.status", $"{status}" },
                { "$top", $"{top}" },
                { "api-version", "6.0" }
            }, TIMEOUT).AsEnumerable();

    public static IEnumerableRequest<GitPullRequest> PullRequests(
        string repositoryId, string status, int top) =>
        new AzdoRequest<GitPullRequest>($"/_apis/git/repositories/{repositoryId}/pullrequests",
            new Dictionary<string, object>
            {
                { "searchCriteria.status", $"{status}" },
                { "$top", $"{top}" },
                { "api-version", "6.0" }
            }, TIMEOUT).AsEnumerable();

    public static IEnumerableRequest<Push> Pushes(string project, string repositoryId) =>
        new AzdoRequest<Push>($"/{project}/_apis/git/repositories/{repositoryId}/pushes").AsEnumerable();

    public static IEnumerableRequest<GitRef> Refs(string project, string repositoryId) =>
        new AzdoRequest<GitRef>($"/{project}/_apis/git/repositories/{repositoryId}/refs").AsEnumerable();

    public static IAzdoRequest<Response.Repository> Repo(string project, string repositoryId) =>
        new AzdoRequest<Response.Repository>($"/{project}/_apis/git/repositories/{repositoryId}");

    public static IEnumerableRequest<Response.Repository> Repositories(string project) =>
        new AzdoRequest<Response.Repository>($"{project}/_apis/git/repositories").AsEnumerable();

    public static IEnumerableRequest<Response.Repository> Repositories() =>
        new AzdoRequest<Response.Repository>($"_apis/git/repositories").AsEnumerable();
}