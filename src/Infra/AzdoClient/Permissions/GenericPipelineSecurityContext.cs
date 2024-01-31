using Microsoft.Extensions.Caching.Memory;
using Rabobank.Compliancy.Infra.AzdoClient.Extensions;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Infra.AzdoClient.Permissions;

/// <summary>
/// SecurityNamespaceContext aimed at a specific instance of a Pipelines in a given Azure DevOps Project.
/// Pipelines can be either of <see cref="BuildDefinition"/> or <see cref="ReleaseDefinition"/> depending on the provided SecurityNamespaceId.
/// </summary>
public class GenericPipelineSecurityContext : ISecurityNamespaceContext
{
    private readonly IAzdoRestClient _client;
    private readonly string _organization;
    private readonly string _projectId;
    private readonly string _securityNamespaceId;
    private readonly string _pipelineId;
    private readonly string _pipelinePath;
    private readonly IMemoryCache _cache;

    /// <summary>
    /// SecurityNamespaceContext aimed at a specific instance of a Pipelines in a given Azure DevOps Project.
    /// Pipelines can be either of <see cref="BuildDefinition"/> or <see cref="ReleaseDefinition"/> depending on the provided <paramref name="securityNamespaceId"/>.
    /// </summary>
    /// <param name="projectId">Defines the Project Scope the Pipeline lives in</param>
    /// <param name="securityNamespaceId">Defines the SecurityNamespaceId which in turn defines whether the context is about <see cref="BuildDefinition"/> or <see cref="ReleaseDefinition"/></param>
    /// <param name="pipelineId"> Unique identifier within the <paramref name="projectId"/> for the Pipeline in scope</param>
    /// <param name="pipelinePath">Folder Path the Pipeline lives in</param>
    public GenericPipelineSecurityContext(IAzdoRestClient client, IMemoryCache cache, string organization,
        string projectId, string securityNamespaceId, string pipelineId, string pipelinePath)
    {
        _client = client;
        _organization = organization;
        _projectId = projectId;
        _securityNamespaceId = securityNamespaceId;
        _pipelineId = pipelineId;
        _pipelinePath = pipelinePath;
        _cache = cache;
    }

    public Task<ApplicationGroups> GetAllApplicationGroupsWithExplicitPermissions()
    {
        var request = Requests.ApplicationGroup.ExplicitIdentitiesPipelines(_projectId, _securityNamespaceId, _pipelineId);
        return _cache.GetOrCreateAsync(_client, request, _organization);
    }

    public Task<ApplicationGroups> GetCollectionOfProjectScopedPermissionGroups()
    {
        var request = Requests.ApplicationGroup.ApplicationGroups(_projectId);
        return _cache.GetOrCreateAsync(_client, request, _organization);
    }

    public Task<PermissionsSet> GetPermissionSetForApplicationGroup(string groupTeamFoundationId)
    {
        if (groupTeamFoundationId == null)
        {
            throw new ArgumentNullException(nameof(groupTeamFoundationId));
        }
        var request = Requests.Permissions.PermissionsGroupSetIdDefinition(_projectId, _securityNamespaceId, groupTeamFoundationId, ExtractToken());
        return _cache.GetOrCreateAsync(_client, request, _organization);
    }

    public Task UpdatePermissionForGroupAsync(string groupTeamFoundationId, PermissionsSet groupPermissionSet, Permission permission)
    {
        if (groupTeamFoundationId == null)
        {
            throw new ArgumentNullException(nameof(groupTeamFoundationId));
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

    private string ExtractToken() =>
        _pipelinePath == "\\" || string.IsNullOrEmpty(_pipelinePath)
            ? $"{_projectId}/{_pipelineId}"
            : $"{_projectId}{_pipelinePath.Replace("\\", "/", StringComparison.InvariantCulture)}/{_pipelineId}";
}