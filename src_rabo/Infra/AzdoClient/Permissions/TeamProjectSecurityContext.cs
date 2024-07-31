using Microsoft.Extensions.Caching.Memory;
using Rabobank.Compliancy.Infra.AzdoClient.Extensions;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Infra.AzdoClient.Permissions;

/// <summary>
/// SecurityNamespaceContext of a specific instance of a <see cref="TeamProjectReference"/> (Azure DevOps Project)
/// </summary>
public class TeamProjectSecurityContext : ISecurityNamespaceContext
{
    private readonly IAzdoRestClient _client;
    private readonly IMemoryCache _cache;
    private readonly string _projectId;
    private readonly string _organization;

    /// <summary>
    /// SecurityNamespaceContext of a specific instance of a <see cref="TeamProjectReference"/> (Azure DevOps Project)
    /// </summary>
    public TeamProjectSecurityContext(
        IAzdoRestClient client, IMemoryCache cache, string organization, string projectId)
    {
        _client = client;
        _cache = cache;
        _organization = organization;
        _projectId = projectId;
    }

    public Task<ApplicationGroups> GetAllApplicationGroupsWithExplicitPermissions() => GetCollectionOfProjectScopedPermissionGroups();

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

        return GetPermissionSetForApplicationGroupInternalAsync(groupTeamFoundationId);
    }

    private async Task<PermissionsSet> GetPermissionSetForApplicationGroupInternalAsync(string groupTeamFoundationId)
    {
        var request = Requests.Permissions.PermissionsGroupProjectId(_projectId, groupTeamFoundationId);
        var permissions = await _cache.GetOrCreateAsync(_client, request, _organization);

        return permissions.Security;
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
                ExtractToken(permission.PermissionToken),
                permission)
            .Wrap();

        return _client.PostAsync(request, body, _organization);
    }

    private static string ExtractToken(string token) =>
        Regex.Match(token, @"^(?:\$PROJECT:)?(.*?)(?::)?$").Groups[1].Value;
}