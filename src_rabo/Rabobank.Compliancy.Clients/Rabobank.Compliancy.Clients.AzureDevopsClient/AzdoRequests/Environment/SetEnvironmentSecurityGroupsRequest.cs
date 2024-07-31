using Microsoft.VisualStudio.Services.Gallery.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Environment.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Environment;

public class SetEnvironmentSecurityGroupsRequest :
    HttpPutRequest<IDevHttpClientCallHandler, PublisherRoleAssignment, RoleAssignmentBodyContent>
{
    private readonly string _identityId;
    private readonly string _organization;
    private readonly string _resourceId;
    private readonly string _scopeId;

    public SetEnvironmentSecurityGroupsRequest(
        string organization,
        string scopeId,
        string resourceId,
        string identityId,
        RoleAssignmentBodyContent value,
        IDevHttpClientCallHandler callHandler) : base(value, callHandler)
    {
        _organization = organization;
        _scopeId = scopeId;
        _resourceId = resourceId;
        _identityId = identityId;
    }

    protected override string Url =>
        $"{_organization}/_apis/securityroles/scopes/{_scopeId}/roleassignments/resources/{_resourceId}/{_identityId}";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.0" }
    };
}