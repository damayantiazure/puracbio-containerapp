using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.TaskGroup;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;

public class TaskGroupRepository : ITaskGroupRepository
{
    private readonly IDevHttpClientCallHandler _httpClientCallHandler;

    public TaskGroupRepository(IDevHttpClientCallHandler httpClientCallHandler)
    {
        _httpClientCallHandler = httpClientCallHandler;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TaskGroup>?> GetTaskGroupsAsync(string organization, Guid projectId, CancellationToken cancellationToken = default)
    {
        var request = new GetAllTaskGroupsForProjectRequest(organization, projectId, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Value;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TaskGroup>?> GetTaskGroupByIdAsync(string organization, Guid projectId, Guid taskGroupId, CancellationToken cancellationToken = default)
    {
        var request = new GetTaskGroupByIdRequest(organization, projectId, taskGroupId, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Value;
    }
}