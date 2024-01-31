using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission;
internal class GetReleaseDefinitionDisplayPermissionRequest : GetDisplayPermissionRequest
{
    private static readonly Guid _releasePermissionSetId = SecurityNamespaces.ReleaseManagement;
    private readonly string _pipelineId;
    private readonly string _pipelinePath;

    public GetReleaseDefinitionDisplayPermissionRequest(
        string organization, Guid projectId, string pipelineId, string pipelinePath, Guid teamFoundationId,
        IDevHttpClientCallHandler httpClientCallHandler)
        : base(organization, projectId, teamFoundationId, _releasePermissionSetId, httpClientCallHandler)
    {
        _pipelineId = pipelineId;
        _pipelinePath = pipelinePath;
    }
    protected override string PermissionSetToken => PermissionExtensions.ExtractPipelinePermissionSetToken(_pipelineId, _pipelinePath, _projectId);
}
