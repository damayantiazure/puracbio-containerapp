using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.WorkItemTracking;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.WorkItemTracking.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;

public class WorkItemTrackingRepository : IWorkItemTrackingRepository
{
    private readonly IDevHttpClientCallHandler _httpClientCallHandler;

    public WorkItemTrackingRepository(IDevHttpClientCallHandler httpClientCallHandler)
    {
        _httpClientCallHandler = httpClientCallHandler;
    }

    /// <inheritdoc/>
    public async Task<WorkItemQueryResult?> GetQueryByWiqlAsync(string organization, Guid projectId, string team, GetQueryBodyContent query, CancellationToken cancellationToken = default)
    {
        var request = new GetQueryByWiqlRequest(organization, projectId, team, query, _httpClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }
}