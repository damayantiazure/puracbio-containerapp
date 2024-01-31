using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Identity;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Identities;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;

public class IdentityRepository : IIdentityRepository
{
    private readonly IVsspsHttpClientCallHandler _httpClientCallHandler;

    public IdentityRepository(IVsspsHttpClientCallHandler httpClientCallHandler)
    {
        _httpClientCallHandler = httpClientCallHandler;
    }

    public async Task<IEnumerable<Identity>?> GetIdentitiesForIdentityDescriptorsAsync(string organization, IEnumerable<IdentityDescriptor> identityDescriptors, QueryMembership queryMembership, CancellationToken cancellationToken = default)
    {
        var request = new GetIdentitiesForIdentityDescriptorsRequest(organization, identityDescriptors, queryMembership, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Value;
    }
}