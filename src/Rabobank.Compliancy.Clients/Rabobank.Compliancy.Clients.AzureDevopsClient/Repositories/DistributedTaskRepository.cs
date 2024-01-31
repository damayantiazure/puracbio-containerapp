using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.DistributedTask;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.DistributedTask.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using System.Text;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;

public class DistributedTaskRepository : IDistributedTaskRepository
{
    private readonly IDevHttpClientCallHandler _httpClientCallHandler;

    public DistributedTaskRepository(IDevHttpClientCallHandler httpClientCallHandler)
    {
        _httpClientCallHandler = httpClientCallHandler;
    }

    /// <inheritdoc/>
    public async Task<string?> AddTaskStartedEventAsync(string organization, Guid projectId, string hubName, Guid planId, AddTaskEventBodyContent taskEvent, CancellationToken cancellationToken = default)
    {
        var request = new AddTaskStartedEventRequest(organization, projectId, hubName, planId, taskEvent, _httpClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<string?> AddTaskCompletedEventAsync(string organization, Guid projectId, string hubName, Guid planId, AddTaskEventBodyContent taskEvent, CancellationToken cancellationToken = default)
    {
        var request = new AddTaskCompletedEventRequest(organization, projectId, hubName, planId, taskEvent, _httpClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<TaskLog?> CreateTaskLogAsync(string organization, Guid projectId, string hubName, Guid planId, TaskLog taskLog, CancellationToken cancellationToken = default)
    {
        var request = new CreateTaskLogRequest(organization, projectId, hubName, planId, taskLog, _httpClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<TaskLog?> AppendToTaskLogAsync(string organization, Guid projectId, string hubName, Guid planId, int logId, string stream, CancellationToken cancellationToken = default)
    {
        byte[] messageBytes = new ASCIIEncoding().GetBytes(stream);
        var request = new AppendToTaskLogRequest(organization, projectId, hubName, planId, logId, messageBytes, _httpClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }
}