using Microsoft.Extensions.Caching.Memory;
using Rabobank.Compliancy.Infra.AzdoClient.Extensions;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Constants;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Threading.Tasks;
using ApplicationGroup = Rabobank.Compliancy.Infra.AzdoClient.Requests.ApplicationGroup;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Infra.AzdoClient.Permissions;

/// <summary>
/// SecurityNamespaceContext of a specific instance of a <see cref="ReleaseDefinitionEnvironment"/> (Stage) of a <see cref="ReleaseDefinition"/> in a given Azure DevOps Project
/// </summary>
public class ClassicReleasePipelineStageSecurityContext : ISecurityNamespaceContext
{
    private readonly IAzdoRestClient _client;
    private readonly IMemoryCache _cache;
    private readonly string _organization;
    private readonly string _projectId;
    private static readonly string _securityNamespaceId = SecurityNamespaceIds.Release;
    private readonly string _pipelineId;
    private readonly string _stageId;
    private readonly string _pipelinePath;

    public ClassicReleasePipelineStageSecurityContext(IAzdoRestClient client, IMemoryCache cache, string organization,
        string projectId, string pipelineId, string stageId, string pipelinePath)
    {
        _client = client;
        _cache = cache;
        _organization = organization;
        _projectId = projectId;
        _pipelineId = pipelineId;
        _stageId = stageId;
        _pipelinePath = pipelinePath;
    }

    public Task<ApplicationGroups> GetAllApplicationGroupsWithExplicitPermissions()
    {
        var request = ApplicationGroup.ExplicitIdentitiesPipelineStage(_projectId, _securityNamespaceId, _pipelineId, _stageId);
        return _cache.GetOrCreateAsync(_client, request, _organization);
    }

    public Task<PermissionsSet> GetPermissionSetForApplicationGroup(string groupTeamFoundationId)
    {
        if (groupTeamFoundationId == null)
        {
            throw new ArgumentNullException(nameof(groupTeamFoundationId));
        }

        var azdoRequest = Requests.Permissions.PermissionsGroupSetIdDefinition(
            _projectId, _securityNamespaceId, groupTeamFoundationId, CreatePermissionSetToken());

        return _cache.GetOrCreateAsync(_client, azdoRequest, _organization);
    }

    public Task<ApplicationGroups> GetCollectionOfProjectScopedPermissionGroups()
    {
        var azdoRequest = ApplicationGroup.ApplicationGroups(_projectId);
        return _cache.GetOrCreateAsync(_client, azdoRequest, _organization);
    }

    public Task UpdatePermissionForGroupAsync(string groupTeamFoundationId, PermissionsSet groupPermissionSet, Permission permission)
    {
        if (groupTeamFoundationId == null)
        {
            throw new ArgumentNullException(nameof(groupTeamFoundationId));
        }

        if (groupPermissionSet == null)
        {
            throw new ArgumentNullException(nameof(groupPermissionSet));
        }

        if (permission == null)
        {
            throw new ArgumentNullException(nameof(permission));
        }

        var request = Requests.Permissions.ManagePermissions(_projectId);
        var body = new ManagePermissionsData(
                groupTeamFoundationId,
                groupPermissionSet.DescriptorIdentifier,
                groupPermissionSet.DescriptorIdentityType,
                permission.PermissionToken,
                permission)
            .Wrap();

        return _client.PostAsync(request, body, _organization);
    }

    private string CreatePermissionSetToken()
    {
        return _pipelinePath == "\\" || string.IsNullOrEmpty(_pipelinePath)
            ? $"{_projectId}/{_pipelineId}/Environment/{_stageId}"
            : $"{_projectId}{_pipelinePath.Replace("\\", "/", StringComparison.InvariantCulture)}/{_pipelineId}/Environment/{_stageId}";
    }
}