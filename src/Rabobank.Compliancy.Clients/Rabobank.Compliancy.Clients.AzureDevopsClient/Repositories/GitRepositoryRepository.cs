using Microsoft.TeamFoundation.SourceControl.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.GitRepository;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;

public class GitRepositoryRepository : IGitRepositoryRepository
{
    private readonly IDevHttpClientCallHandler _httpClientCallHandler;

    public GitRepositoryRepository(IDevHttpClientCallHandler httpClientCallHandler)
    {
        _httpClientCallHandler = httpClientCallHandler;
    }

    /// <inheritdoc />
    public async Task<GitRepository?> GetGitRepoByIdAsync(string organization, Guid projectId, Guid gitRepoId, CancellationToken cancellationToken = default)
    {
        var request = new GetGitRepositoryRequest(organization, projectId, gitRepoId, _httpClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<GitRepository?> GetGitRepoByNameAsync(string organization, string projectName, string gitRepoName, CancellationToken cancellationToken = default)
    {
        var request = new GetGitRepositoryRequest(organization, projectName, gitRepoName, _httpClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<GitRepository>?> GetGitReposByProjectAsync(string organization, Guid projectId, CancellationToken cancellationToken = default)
    {
        var request = new GetAllGitRepositoriesForProjectRequest(organization, projectId, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Value;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<GitPullRequest>?> GetAllPullRequestsAsync(string organization, Guid projectId, Guid repositoryId, CancellationToken cancellationToken = default)
    {
        var request = new GetAllPullRequestsRequest(organization, projectId, repositoryId, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Value;
    }

    /// <inheritdoc />
    public async Task<GitPullRequest?> GetPullRequestAsync(string organization, Guid projectId, Guid repositoryId, int pullRequestId, CancellationToken cancellationToken = default)
    {
        var request = new GetPullRequestRequest(organization, projectId, repositoryId, pullRequestId, _httpClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IdentityRefWithVote>?> GetPullRequestReviewersAsync(string organization, Guid projectId, Guid repositoryId, int pullRequestId, CancellationToken cancellationToken = default)
    {
        var request = new GetPullRequestReviewersRequest(organization, projectId, repositoryId, pullRequestId, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Value;
    }
}