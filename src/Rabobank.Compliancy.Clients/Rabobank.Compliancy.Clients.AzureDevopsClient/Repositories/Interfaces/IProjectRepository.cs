using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Operations;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;

/// <summary>
/// Provides methods to cater to all object needs from the Azure Devops API regarding Projects
/// </summary>
public interface IProjectRepository
{
    /// <summary>
    /// Gets a project by ID.
    /// </summary>
    /// <param name="organization">The organization the project belongs to.</param>
    /// <param name="id">The ID of the project.</param>
    /// <param name="includeCapabilities">Include capabilities (such as source control) in the team project result.</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable <see cref="TeamProject"/> representing a Project the way Azure Devops API returns it.</returns>
    Task<TeamProject?> GetProjectByIdAsync(string organization, Guid id, bool includeCapabilities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a project by project name.
    /// </summary>
    /// <param name="organization">The organization the project belongs to.</param>
    /// <param name="projectName">The name of the the project.</param>
    /// <param name="includeCapabilities">Include capabilities (such as source control) in the team project result.</param>
    /// <param name="cancellationToken">Cancels the API call if necessary.</param>
    /// <returns>Nullable <see cref="TeamProject"/> representing a Project the way Azure Devops API returns it.</returns>
    Task<TeamProject?> GetProjectByNameAsync(string organization, string projectName, bool includeCapabilities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new project.
    /// </summary>
    /// <param name="organization">The organization that will be used to create the project.</param>
    /// <param name="projectName">The name that will be used to create the project.</param>
    /// <param name="description">The description that will be applied to the project.</param>
    /// <param name="cancellationToken">Cancels the API call if necessary.</param>
    /// <returns>Nullable <see cref="TeamProject"/> representing a Project the way Azure Devops API returns it.</returns>
    Task<Operation?> CreateProjectAsync(string organization, string projectName, string description, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a project by using the project identifier.
    /// </summary>
    /// <param name="organization">The organization the project belongs to.</param>
    /// <param name="id">The identifier of the project.</param>
    /// <param name="cancellationToken">Cancels the API call if necessary.</param>
    /// <returns>Nullable <see cref="Operation"/> that defines if the execution is succesful.</returns>
    Task<Operation?> DeleteProjectAsync(string organization, Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all projects for this organization.
    /// </summary>
    /// <param name="organization">The organization the projects belongs to.</param>
    /// <param name="cancellationToken">Cancels the API call if necessary.</param>
    /// <returns>Nullable IEnumerable of <see cref="TeamProject"/> representing a Project the way Azure Devops API returns it.</returns>
    Task<IEnumerable<TeamProject>?> GetProjectsAsync(string organization, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the properties of a project
    /// </summary>
    /// <param name="organization">The organization the project belongs to.</param>
    /// <param name="projectId">The identifier of the project.</param>
    /// <param name="cancellationToken">Cancels the API call if necessary.</param>
    /// <returns>Nullable IEnumerable of <see cref="ProjectProperty"/> representing Project Properties the way Azure Devops API returns it.</returns>
    Task<IEnumerable<ProjectProperty>?> GetProjectPropertiesAsync(string organization, Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the projectInfo
    /// </summary>
    /// <param name="organization">The organization the project belongs to.</param>
    /// <param name="projectId">The identifier of the project.</param>
    /// <param name="cancellationToken">Cancels the API call if necessary.</param>
    /// <returns>Nullable <see cref="ProjectInfo"/> that defines if the execution is succesful.</returns>
    Task<ProjectInfo?> GetProjectInfoAsync(string organization, Guid projectId, CancellationToken cancellationToken = default);
}