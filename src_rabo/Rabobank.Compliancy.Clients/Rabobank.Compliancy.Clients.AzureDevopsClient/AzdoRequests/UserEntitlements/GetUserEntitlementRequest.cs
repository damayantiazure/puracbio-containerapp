using Microsoft.VisualStudio.Services.MemberEntitlementManagement.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.UserEntitlements;

/// <inheritdoc/>
internal class GetUserEntitlementRequest : HttpGetRequest<IVsaexHttpClientCallHandler, UserEntitlement>
{
    private readonly string _organization;
    private readonly Guid _userId;

    protected override string? Url => $"{_organization}/_apis/userentitlements/{_userId}";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.0" },
    };

    public GetUserEntitlementRequest(string organization, Guid userId, IVsaexHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _userId = userId;
    }
}