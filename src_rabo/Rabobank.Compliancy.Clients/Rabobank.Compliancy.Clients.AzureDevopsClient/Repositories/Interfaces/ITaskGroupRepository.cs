using Microsoft.TeamFoundation.DistributedTask.WebApi;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;

/// <summary>
/// Provides methods to cater to all object needs from the Azure Devops API regarding <see cref="TaskGroup"/>
/// </summary>
public interface ITaskGroupRepository
{
    /// <summary>
    /// Gets all TaskGroups by projectId.
    /// </summary>
    /// <param name="organization">The organization the TaskGroups belongs to</param>
    /// <param name="projectId">The project the TaskGroups belong to</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable <see cref="TaskGroup"/> representing a TaskGroup the way Azure Devops API returns it.</returns>
    Task<IEnumerable<TaskGroup>?> GetTaskGroupsAsync(string organization, Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets TaskGroup by taskGroupId.
    /// </summary>
    /// <param name="organization">The organization the TaskGroups belongs to</param>
    /// <param name="projectId">The project the TaskGroups belong to</param>
    /// <param name="taskGroupId">Id of the task group.</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable <see cref="TaskGroup"/> representing a TaskGroup the way Azure Devops API returns it.</returns>
    Task<IEnumerable<TaskGroup>?> GetTaskGroupByIdAsync(string organization, Guid projectId, Guid taskGroupId, CancellationToken cancellationToken = default);
}