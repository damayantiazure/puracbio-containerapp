using Microsoft.VisualStudio.Services.MemberEntitlementManagement.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.UserEntitlements;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;

/// <inheritdoc/>
public class UserEntitlementRepository : IUserEntitlementRepository
{
    private readonly IVsaexHttpClientCallHandler _httpClientCallHandler;

    public UserEntitlementRepository(IVsaexHttpClientCallHandler httpClientCallHandler)
    {
        _httpClientCallHandler = httpClientCallHandler;
    }

    /// <inheritdoc/>
    public async Task<UserEntitlement?> GetUserEntitlementByIdAsync(string organization, Guid userId, CancellationToken cancellationToken = default)
    {
        var request = new GetUserEntitlementRequest(organization, userId, _httpClientCallHandler);
        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }
}