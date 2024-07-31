using Microsoft.Extensions.Caching.Memory;
using Rabobank.Compliancy.Infra.AzdoClient.Extensions;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Constants;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Infra.AzdoClient.Permissions;

/// <summary>
/// SecurityNamespaceContext of a specific instance of a <see cref="Repository"/> in a given Azure DevOps Project
/// </summary>
public class RepositorySecurityContext : ISecurityNamespaceContext
{
    private readonly IAzdoRestClient _legacyAzdoClient;
    private readonly string _organization;
    private readonly string _projectId;
    private readonly string _repositoryId;
    private readonly IMemoryCache _cache;

    /// <summary>
    /// SecurityNamespaceContext of a specific instance of a <see cref="Repository"/> in a given Azure DevOps Project
    /// </summary>
    public RepositorySecurityContext(
        IAzdoRestClient client, IMemoryCache cache, string organization, string projectId, string repositoryId)
    {
        _cache = cache;
        _legacyAzdoClient = client;
        _organization = organization;
        _projectId = projectId;
        _repositoryId = repositoryId;
    }

    public Task<ApplicationGroups> GetAllApplicationGroupsWithExplicitPermissions()
    {
        var request = Requests.ApplicationGroup.ExplicitIdentitiesRepos(_projectId, SecurityNamespaceIds.GitRepositories, _repositoryId);
        return _cache.GetOrCreateAsync(_legacyAzdoClient, request, _organization);
    }

    public Task<ApplicationGroups> GetCollectionOfProjectScopedPermissionGroups()
    {
        var request = Requests.ApplicationGroup.ApplicationGroups(_projectId);
        return _cache.GetOrCreateAsync(_legacyAzdoClient, request, _organization);
    }

    public Task<PermissionsSet> GetPermissionSetForApplicationGroup(string groupTeamFoundationId)
    {
        if (groupTeamFoundationId == null)
        {
            throw new ArgumentNullException(nameof(groupTeamFoundationId));
        }

        var request = Requests.Permissions.PermissionsGroupRepository(_projectId, groupTeamFoundationId, _repositoryId);

        return _cache.GetOrCreateAsync(_legacyAzdoClient, request, _organization);
    }

    public Task UpdatePermissionForGroupAsync(string groupTeamFoundationId, PermissionsSet groupPermissionSet, Permission permission)
    {
        if (groupTeamFoundationId == null)
        {
            throw new ArgumentNullException(nameof(groupTeamFoundationId));
        }

        return _legacyAzdoClient.PostAsync(Requests.Permissions.ManagePermissions(_projectId),
            new ManagePermissionsData(groupTeamFoundationId, groupPermissionSet.DescriptorIdentifier,
                groupPermissionSet.DescriptorIdentityType, permission.PermissionToken, permission).Wrap(),
            _organization);
    }
}