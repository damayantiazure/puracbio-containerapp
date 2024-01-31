using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.DistributedTask.Models;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;

/// <summary>
/// Provides methods to cater to all object needs from the Azure Devops API regarding <see cref="DistributedTaskRepository"/>
/// </summary>
public interface IDistributedTaskRepository
{
    /// <summary>
    /// Adds a task started event
    /// </summary>
    /// <param name="organization">The organization the DistributedTask belongs to</param>
    /// <param name="projectId">The project the DistributedTask belongs to</param>
    /// <param name="hubName">The name of the server hub</param>
    /// <param name="planId">The ID of the plan</param>
    /// <param name="taskEvent">Job event data to be added</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable string representing whether adding started task was successful.</returns>
    Task<string?> AddTaskStartedEventAsync(string organization, Guid projectId, string hubName, Guid planId, AddTaskEventBodyContent taskEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a task completed event
    /// </summary>
    /// <param name="organization">The organization the DistributedTask belongs to</param>
    /// <param name="projectId">The project the DistributedTask belongs to</param>
    /// <param name="hubName">The name of the server hub</param>
    /// <param name="planId">The ID of the plan</param>
    /// <param name="taskEvent">Task event data to be added</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable string representing whether adding completed task was successful.</returns>
    Task<string?> AddTaskCompletedEventAsync(string organization, Guid projectId, string hubName, Guid planId, AddTaskEventBodyContent taskEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a log and connects it to a pipeline run's execution plan
    /// </summary>
    /// <param name="organization">The organization the DistributedTask belongs to</param>
    /// <param name="projectId">The project the DistributedTask belongs to</param>
    /// <param name="hubName">The name of the server hub</param>
    /// <param name="planId">The ID of the plan</param>
    /// <param name="taskLog">TaskLog the way Azure Devops API understands it</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable <see cref="TaskLog"/> representing a TaskLog the way Azure Devops API returns it.</returns>
    Task<TaskLog?> CreateTaskLogAsync(string organization, Guid projectId, string hubName, Guid planId, TaskLog taskLog, CancellationToken cancellationToken = default);

    /// <summary>
    /// Appends a log to a task's log
    /// </summary>
    /// <param name="organization">The organization the DistributedTask belongs to</param>
    /// <param name="projectId">The project the DistributedTask belongs to</param>
    /// <param name="hubName">The name of the server hub</param>
    /// <param name="planId">The ID of the plan</param>
    /// <param name="logId">The ID of the log</param>
    /// <param name="stream">Stream to upload</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable <see cref="TaskLog"/> representing a TaskLog the way Azure Devops API returns it.</returns>
    Task<TaskLog?> AppendToTaskLogAsync(string organization, Guid projectId, string hubName, Guid planId, int logId, string stream, CancellationToken cancellationToken = default);
}