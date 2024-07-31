using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission;

internal class GetApplicationGroupDisplayPermissionsRequest : GetDisplayPermissionRequest
{
    public GetApplicationGroupDisplayPermissionsRequest(string organization, Guid projectId, Guid applicationGroupId,
        IDevHttpClientCallHandler httpClientCallHandler) : base(organization, projectId, applicationGroupId, SecurityNamespaces.Build, httpClientCallHandler)
    {
    }

    protected override string PermissionSetToken => _projectId.ToString();
}