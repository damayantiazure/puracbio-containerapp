using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.ApplicationGroup;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;

/// <inheritdoc/>
public class ApplicationGroupRepository : IApplicationGroupRepository
{
    private readonly IDevHttpClientCallHandler _httpClientCallHandler;

    public ApplicationGroupRepository(IDevHttpClientCallHandler httpClientCallHandler) =>
        _httpClientCallHandler = httpClientCallHandler;

    /// <inheritdoc/>
    public async Task<ApplicationGroup?> GetApplicationGroupForRepositoryAsync(string organization, Guid projectId, Guid repositoryId, CancellationToken cancellationToken)
    {
        return await new GetApplicationGroupForRepositoryRequest(organization, projectId, repositoryId, _httpClientCallHandler)
            .ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ApplicationGroup>?> GetApplicationGroupsForGroupAsync(string organization, Guid groupId, CancellationToken cancellationToken)
    {
        var request = new GetApplicationGroupsForGroupRequest(organization, groupId, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Value;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ApplicationGroup>?> GetApplicationGroupsForProjectAsync(string organization, Guid projectId, CancellationToken cancellationToken)
    {
        var request = new GetApplicationGroupsForProjectRequest(organization, projectId, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Value;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ApplicationGroup>?> GetApplicationGroupsAsync(string organization, CancellationToken cancellationToken)
    {
        var request = new GetApplicationGroupsRequest(organization, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Value;
    }

    /// <inheritdoc/>
    public async Task<ApplicationGroup?> GetScopedApplicationGroupForProjectAsync(string organization, Guid projectId, CancellationToken cancellationToken)
    {
        return await new GetScopedApplicationGroupRequest(organization, projectId, _httpClientCallHandler)
            .ExecuteAsync(cancellationToken: cancellationToken);
    }
}