using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Infra.AzdoClient.Permissions;

/// <summary>
/// The AzureDevOps security context within which <see cref="Permission"/> Operations can be executed.
/// This context is always within a defined Namespace
/// </summary>
public interface ISecurityNamespaceContext
{
    /// <summary>
    /// Get all PermissionGroups for the current Project Scope. Does not include groups from other scopes like Organization, Azure AD and other projects.
    /// </summary>
    /// <returns></returns>
    Task<ApplicationGroups> GetCollectionOfProjectScopedPermissionGroups();
    /// <summary>
    /// Retrieves all <see cref="ApplicationGroup"/>s that have explicit <see cref="Permission"/>s assigned to them for the current <see cref="ISecurityNamespaceContext"/>
    /// </summary>
    /// <returns></returns>
    Task<ApplicationGroups> GetAllApplicationGroupsWithExplicitPermissions();
    /// <summary>
    /// Gets the PermissionSet for the <paramref name="groupTeamFoundationId"/> in the current <see cref="ISecurityNamespaceContext"/>
    /// </summary>
    /// <param name="groupTeamFoundationId">TeamFoundationId of the PermissionGroup</param>
    /// <returns></returns>
    Task<PermissionsSet> GetPermissionSetForApplicationGroup(string groupTeamFoundationId);
    /// <summary>
    /// Updates a specific <see cref="Permission"/> for a given <see cref="ApplicationGroup"/>
    /// </summary>
    /// <param name="groupTeamFoundationId">Represents an <see cref="ApplicationGroup.TeamFoundationId"/> property</param>
    /// <param name="groupPermissionSet">Should be the <see cref="PermissionsSet"/> belonging to <paramref name="groupTeamFoundationId"/> within the same <see cref="ISecurityNamespaceContext"/></param>
    /// <param name="permission"><see cref="Permission"/> to be Set</param>
    /// <returns></returns>
    Task UpdatePermissionForGroupAsync(string groupTeamFoundationId, PermissionsSet groupPermissionSet, Permission permission);
}