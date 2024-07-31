using Microsoft.VisualStudio.Services.Security;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.AccessControlLists;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.GitRepository;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;

public class AccessControlListsRepository : IAccessControlListsRepository
{
    private readonly IDevHttpClientCallHandler _httpClientCallHandler;

    public AccessControlListsRepository(IDevHttpClientCallHandler httpClientCallHandler)
    {
        _httpClientCallHandler = httpClientCallHandler;
    }

    public async Task<IEnumerable<AccessControlList>?> GetAccessControlListsForProjectAndSecurityNamespaceAsync(string organization, Guid projectId, Guid securityNamespaceId, CancellationToken cancellationToken = default)
    {
        var request = new GetAccessControlListsForProjectAndSecurityNamespaceRequest(organization, projectId, securityNamespaceId, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Value;
    }
}