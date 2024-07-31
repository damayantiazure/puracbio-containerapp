using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Application.Services;

/// <summary>
/// Service defintions for the <see cref="GitRepo"/>s.
/// </summary>
public interface IGitRepoService
{
    /// <summary>
    /// Retrieves the git repository by using the repository identifier.
    /// </summary>
    /// <param name="project">An instance of the project as <see cref="Project"/>.</param>
    /// <param name="gitRepoId">The identifier of the repository.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>An instance of <see cref="GitRepo"/>.</returns>
    Task<GitRepo> GetGitRepoByIdAsync(Project project, Guid gitRepoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the git repository by using the repository name.
    /// </summary>
    /// <param name="project">An instance of the project as <see cref="Project"/>.</param>
    /// <param name="gitRepoName">The name of the repository.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>An instance of <see cref="GitRepo"/>.</returns>
    Task<GitRepo> GetGitRepoByNameAsync(Project project, string gitRepoName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add permissions that users or groups may have to this gitrepo. 
    /// Requires aditional requests to underlaying information provider client.
    /// </summary>
    /// <param name="gitRepo">The gitrepo the permissions are for.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>An instance of <see cref="Project"/>.</returns>
    Task<GitRepo> AddPermissionsAsync(GitRepo gitRepo, CancellationToken cancellationToken = default);
}