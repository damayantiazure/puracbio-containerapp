using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;

/// <summary>
/// Provides methods to cater to all project permissions.
/// </summary>
public interface IApplicationGroupRepository
{
    /// <summary>
    /// Fetches a single applicationgroup by project and repository
    /// </summary>
    /// <param name="organization">The organization the ApplicationGroup belongs to</param>
    /// <param name="projectId">The project the ApplicationGroup belongs to</param>
    /// <param name="repositoryId">The repository the ApplicationGroup belongs to</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    Task<ApplicationGroup?> GetApplicationGroupForRepositoryAsync(string organization, Guid projectId, Guid repositoryId, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches a collection of applicationgroups for the entire organization
    /// </summary>
    /// <param name="organization">The organizations the ApplicationGroup belongs to</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    Task<IEnumerable<ApplicationGroup>?> GetApplicationGroupsAsync(string organization, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches a collection of applicationgroups by group
    /// </summary>
    /// <param name="organization">The organization the ApplicationGroups belongs to</param>
    /// <param name="groupId">The group the applicationgroups fall under</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    Task<IEnumerable<ApplicationGroup>?> GetApplicationGroupsForGroupAsync(string organization, Guid groupId, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches a collection of applicationgroups by project
    /// </summary>
    /// <param name="organization">The organization the ApplicationGroups belongs to</param>
    /// <param name="projectId">The project the ApplicationGroups belongs to</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    Task<IEnumerable<ApplicationGroup>?> GetApplicationGroupsForProjectAsync(string organization, Guid projectId, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches a single applicationgroup scoped by project
    /// </summary>
    /// <param name="organization">The organization the ApplicationGroup belongs to</param>
    /// <param name="projectId">The project the ApplicationGroup belongs to</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns></returns>
    Task<ApplicationGroup?> GetScopedApplicationGroupForProjectAsync(string organization, Guid projectId, CancellationToken cancellationToken);
}