using Microsoft.VisualStudio.Services.Location;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Authorization;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;

public class AuthorizationRepository : IAuthorizationRepository
{
    private readonly IDevHttpClientCallHandler _httpClientCallHandler;

    public AuthorizationRepository(IDevHttpClientCallHandler httpClientCallHandler)
    {
        _httpClientCallHandler = httpClientCallHandler;
    }

    public async Task<ConnectionData> GetUserForAccessToken(AuthenticationHeaderValue authenticationHeaderValue, string organization, CancellationToken cancellationToken = default)
    {
        var request = new GetConnectionDataRequest(organization, _httpClientCallHandler);
        return await request.ExecuteAsync(authenticationHeaderValue, cancellationToken) ?? new ConnectionData();
    }
}