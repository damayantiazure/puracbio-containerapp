using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission;

internal class GetRepositoryDisplayPermissionsRequest : GetDisplayPermissionRequest
{
    private readonly Guid _repositoryId;

    public GetRepositoryDisplayPermissionsRequest(string organization, Guid projectId, Guid repositoryId, Guid teamFoundationId,
        IDevHttpClientCallHandler httpClientCallHandler) : base(organization, projectId, teamFoundationId, SecurityNamespaces.GitRepo, httpClientCallHandler)
    {
        _repositoryId = repositoryId;
    }

    protected override string PermissionSetToken => $"repoV2/{_projectId}/{_repositoryId}";
}