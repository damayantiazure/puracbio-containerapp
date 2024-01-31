using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;

/// <summary>
/// Provides methods to cater to all object needs from the Azure Devops API regarding <see cref="GitRepository"/>
/// </summary>
public interface IGitRepositoryRepository
{
    /// <summary>
    /// Gets a gitRepo by ID.
    /// </summary>
    /// <param name="organization">The organization the gitRepo belongs to</param>
    /// <param name="projectId">The project the GitRepo belongs to</param>
    /// <param name="gitRepoId">The ID of the GitRepo</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable <see cref="GitRepository"/> representing a GitRepo the way Azure Devops API returns it.</returns>
    Task<GitRepository?> GetGitRepoByIdAsync(string organization, Guid projectId, Guid gitRepoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a gitRepo by name.
    /// </summary>
    /// <param name="organization">The organization the gitRepo belongs to</param>
    /// <param name="projectName">The project the GitRepo belongs to</param>
    /// <param name="gitRepoName">The name of the GitRepo</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable <see cref="GitRepository"/> representing a GitRepo the way Azure Devops API returns it.</returns>
    Task<GitRepository?> GetGitRepoByNameAsync(string organization, string projectName, string gitRepoName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all gitRepos by project
    /// </summary>
    /// <param name="organization">The organization the project belongs to</param>
    /// <param name="projectId">The project the GitRepos belongs to</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable enumerable of <see cref="GitRepository"/> representing a GitRepo the way Azure Devops API returns it.</returns>
    Task<IEnumerable<GitRepository>?> GetGitReposByProjectAsync(string organization, Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all pullrequests by repository
    /// </summary>
    /// <param name="organization">The organization the project belongs to</param>
    /// <param name="projectId">The project the GitRepos belongs to</param>
    /// <param name="repositoryId">The repository the pullrequests belong to</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns></returns>
    Task<IEnumerable<GitPullRequest>?> GetAllPullRequestsAsync(string organization, Guid projectId, Guid repositoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a pullrequest by ID
    /// </summary>
    /// <param name="organization">The organization the project belongs to</param>
    /// <param name="projectId">The project the GitRepos belongs to</param>
    /// <param name="repositoryId">The repository the pullrequests belong to</param>
    /// <param name="pullRequestId">The pullrequest that is being fetched</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns></returns>
    Task<GitPullRequest?> GetPullRequestAsync(string organization, Guid projectId, Guid repositoryId, int pullRequestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all reviewers by pullrequest
    /// </summary>
    /// <param name="organization">The organization the project belongs to</param>
    /// <param name="projectId">The project the GitRepos belongs to</param>
    /// <param name="repositoryId">The repository the pullrequests belong to</param>
    /// <param name="pullRequestId">The pullrequest the reviewers have reviewd</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns></returns>
    Task<IEnumerable<IdentityRefWithVote>?> GetPullRequestReviewersAsync(string organization, Guid projectId, Guid repositoryId, int pullRequestId, CancellationToken cancellationToken = default);
}