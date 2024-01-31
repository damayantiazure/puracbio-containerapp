using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Application.Services;

/// <summary>
/// Service defintions for the <see cref="Project"/>s.
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// Get project by using the project identifier.
    /// </summary>
    /// <param name="organization">The name of the organization.</param>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>An instance of <see cref="Project"/>.</returns>
    Task<Project> GetProjectByIdAsync(string organization, Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get project by using the name of the project.
    /// </summary>
    /// <param name="organization">The name of the organization.</param>
    /// <param name="projectName">The name of the project.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>An instance of <see cref="Project"/>.</returns>
    Task<Project> GetProjectByNameAsync(string organization, string projectName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add permissions that users or groups may have to this project. 
    /// Requires aditional requests to underlaying information provider client.
    /// </summary>
    /// <param name="project">The project the permissions are for.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>An instance of <see cref="Project"/>.</returns>
    Task<Project> AddPermissionsAsync(Project project, CancellationToken cancellationToken = default);
}