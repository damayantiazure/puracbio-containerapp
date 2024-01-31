using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Microsoft.VisualStudio.Services.Gallery.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Environment;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Environment.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;

public class EnvironmentRepository : IEnvironmentRepository
{
    private readonly IDevHttpClientCallHandler _httpClientCallHandler;

    public EnvironmentRepository(IDevHttpClientCallHandler httpClientCallHandler)
    {
        _httpClientCallHandler = httpClientCallHandler;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EnvironmentInstance>?> GetEnvironmentsAsync(
        string organization, Guid projectId, CancellationToken cancellationToken = default)
    {
        var request = new GetEnvironmentInstancesRequest(organization, projectId, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Value;
    }

    /// <inheritdoc />
    public async Task<PublisherRoleAssignment?> SetSecurityGroupsAsync(
        string organization, string scopeId, string resourceId, string identityId, RoleAssignmentBodyContent content,
        CancellationToken cancellationToken = default)
    {
        var request = new SetEnvironmentSecurityGroupsRequest(organization, scopeId, resourceId, identityId, content,
            _httpClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }
}