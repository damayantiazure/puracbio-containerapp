using Microsoft.VisualStudio.Services.Gallery.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Environment;

public class GetEnvironmentSecurityGroupsRequest :
        HttpGetRequest<IDevHttpClientCallHandler, ResponseCollection<PublisherRoleAssignment>>
{
    private readonly string _organization;
    private readonly string _resourceId;
    private readonly string _scopeId;

    public GetEnvironmentSecurityGroupsRequest(string organization, string scopeId, string resourceId,
        IDevHttpClientCallHandler callHandler) : base(callHandler)
    {
        _organization = organization;
        _scopeId = scopeId;
        _resourceId = resourceId;
    }

    protected override string Url =>
        $"{_organization}/_apis/securityroles/scopes/{_scopeId}/roleassignments/resources/{_resourceId}";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.0" }
    };
}