using Microsoft.Extensions.Caching.Memory;
using Rabobank.Compliancy.Infra.AzdoClient.Extensions;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Infra.AzdoClient.Permissions;

/// <summary>
/// SecurityNamespaceContext aimed at the Root Folder of all Pipelines in a given Azure DevOps Project.
/// Pipelines can be either of <see cref="BuildDefinition"/> or <see cref="ReleaseDefinition"/> depending on the provided SecurityNamespaceId.
/// </summary>
public class GenericPipelineRootFolderSecurityContext : ISecurityNamespaceContext
{
    private readonly IAzdoRestClient _client;
    private readonly string _projectId;
    private readonly string _organization;
    private readonly string _namespaceId;
    private readonly IMemoryCache _cache;

    /// <summary>
    /// SecurityNamespaceContext aimed at the Root Folder of all Pipelines in a given Azure DevOps Project.
    /// Pipelines can be either of <see cref="BuildDefinition"/> or <see cref="ReleaseDefinition"/> depending on the provided <paramref name="securityNamespaceId"/>.
    /// </summary>
    /// <param name="projectId">Defines the Project Scope the Pipeline lives in</param>
    /// <param name="securityNamespaceId">Defines the SecurityNamespaceId which in turn defines whether the context is about <see cref="BuildDefinition"/> or <see cref="ReleaseDefinition"/></param>
    public GenericPipelineRootFolderSecurityContext(IAzdoRestClient client, IMemoryCache cache, string organization,
        string projectId, string securityNamespaceId)
    {
        _client = client;
        _projectId = projectId;
        _organization = organization;
        _namespaceId = securityNamespaceId;
        _cache = cache;
    }

    public Task<Response.ApplicationGroups> GetAllApplicationGroupsWithExplicitPermissions() =>
        _cache.GetOrCreateAsync(_client, Requests.ApplicationGroup.ExplicitIdentitiesPipelines(
            _projectId, _namespaceId), _organization);

    public Task<ApplicationGroups> GetCollectionOfProjectScopedPermissionGroups() =>
        _cache.GetOrCreateAsync(_client, Requests.ApplicationGroup.ApplicationGroups(
            _projectId), _organization);

    public Task<Response.PermissionsSet> GetPermissionSetForApplicationGroup(string groupTeamFoundationId)
    {
        if (groupTeamFoundationId == null)
        {
            throw new ArgumentNullException(nameof(groupTeamFoundationId));
        }
        var request = Requests.Permissions.PermissionsGroupSetId(_projectId, _namespaceId, groupTeamFoundationId);

        return _cache.GetOrCreateAsync(_client, request, _organization);
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
}