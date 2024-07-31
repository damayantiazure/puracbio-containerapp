using Microsoft.VisualStudio.Services.Security;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.AccessControlLists;

/// <summary>
/// Used to get an access control list for a specific project and security namespace, using the url "{_organization}/_apis/accesscontrollists/{_securityNamespaceId}".
/// </summary>
public class GetAccessControlListsForProjectAndSecurityNamespaceRequest : HttpGetRequest<IDevHttpClientCallHandler, ResponseCollection<AccessControlList>>
{
    private readonly string _organization;
    private readonly string _projectId;
    private readonly string _securityNamespaceId;

    protected override string Url => $"{_organization}/_apis/accesscontrollists/{_securityNamespaceId}";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        {"api-version", "7.0"},
        {"includeExtendedInfo", "true"},
        {"token", $"$PROJECT:vstfs:///Classification/TeamProject/{_projectId}"}
    };

    public GetAccessControlListsForProjectAndSecurityNamespaceRequest(string organization, Guid projectId, Guid securityNamespaceId,
        IDevHttpClientCallHandler httpClientCallHandler) : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId.ToString();
        _securityNamespaceId = securityNamespaceId.ToString();
    }
}