using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission;

/// <summary>
/// NOTE: This endpoint is a legacy endpoint of microsoft and needs to be replaced with a
/// more recent endpoint that is documented on the azure devops api website. <see cref="https://learn.microsoft.com/en-us/rest/api/azure/devops/?view=azure-devops-rest-7.1"/>
/// </summary>
internal abstract class GetDisplayPermissionRequest : HttpGetRequest<IDevHttpClientCallHandler, PermissionsSet>
{
    protected readonly string _organization;
    protected readonly Guid _projectId;
    protected readonly Guid _teamFoundationId;
    protected readonly Guid _securityNamespaceId;

    protected GetDisplayPermissionRequest(string organization, Guid projectId, Guid teamFoundationId, Guid securityNamespaceId,
        IDevHttpClientCallHandler httpClientCallHandler) : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
        _teamFoundationId = teamFoundationId;
        _securityNamespaceId = securityNamespaceId;
    }

    protected override string Url => $"{_organization}/{_projectId}/_api/_security/DisplayPermissions";

    protected abstract string PermissionSetToken { get; }

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "__v", "5" },
        { "tfid", $"{_teamFoundationId}" },
        { "permissionSetId", $"{_securityNamespaceId}" },
        { "permissionSetToken", PermissionSetToken }
    };
}