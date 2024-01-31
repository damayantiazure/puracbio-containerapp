using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;

/// <summary>
/// Provides methods to cater to all project permissions.
/// </summary>
public interface IPermissionRepository
{
    /// <summary>
    /// Creates a new project group.
    /// </summary>
    /// <param name="organization">The organization the project group belongs to.</param>
    /// <param name="projectId">The ID of the project.</param>
    /// <param name="groupName">The name of the group to be created.</param>
    /// <param name="cancellationToken">Cancels the API call if necessary.</param>
    /// <returns>Nullable <see cref="Group"/> representing a Group the way Azure Devops API returns it.</returns>
    Task<Group?> CreateManageGroupAsync(string organization, Guid projectId, string? groupName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get project group by using the project identifier.
    /// </summary>
    /// <param name="organization">The organization the project group belongs to.</param>
    /// <param name="projectId">The ID of the project.</param>
    /// <param name="cancellationToken">Cancels the API call if necessary.</param>
    /// <returns>Nullable <see cref="ProjectGroup"/> representing a project group.</returns>
    Task<ProjectGroup?> GetProjectGroupAsync(string organization, Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add members to a project group.
    /// </summary>
    /// <param name="organization">The organization the project group belongs to.</param>
    /// <param name="projectId">The ID of the project.</param>
    /// <param name="users">The list of user identifiers.</param>
    /// <param name="groups">The list of the project group identifiers.</param>
    /// <param name="cancellationToken">Cancels the API call if necessary.</param>
    /// <returns>Nullable <see cref="MembersGroupResponse"/> representing a MembersGroup.</returns>
    Task<MembersGroupResponse?> AddMembersToGroupsAsync(string organization, Guid projectId, IEnumerable<Guid> users, IEnumerable<Guid> groups, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Permissions for a user/group.
    /// </summary>
    /// <param name="organization">The organization the user/group belongs to.</param>
    /// <param name="projectId">The ID of the project.</param>
    /// <param name="id">The ID of the user or group.</param>
    /// <param name="cancellationToken">Cancels the API call if necessary.</param>
    /// <returns>Nullable <see cref="PermissionsProjectId"/> representing a permission project.</returns>
    Task<PermissionsProjectId?> GetPermissionsUserOrGroupAsync(string organization, Guid projectId, Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a permission group.
    /// </summary>
    /// <param name="organization">The organization the permission group belongs to</param>
    /// <param name="projectId">The project the permission group belongs to</param>
    /// <param name="updatePermissionBodyContent">Permission group content to be updated</param>
    /// <param name="cancellationToken">Cancels the API call if necessary.</param>
    /// <returns>Nullable <see cref="PermissionsSet"/> representing a set of permissions.</returns>
    Task<PermissionsSet?> UpdatePermissionGroupAsync(string organization, Guid projectId, UpdatePermissionBodyContent updatePermissionBodyContent,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the displaypermissions for a GitRepository
    /// </summary>
    /// <param name="organization">The organization the repository belongs to</param>
    /// <param name="projectId">The project the repository belongs to</param>
    /// <param name="repositoryId">The ID of the repository</param>
    /// <param name="teamsFoundationId">The TeamFoundationID of passed along with the request</param>
    /// <param name="cancellationToken">Cancels the API call if necessary.</param>
    /// <returns>Nullable <see cref="PermissionsSet"/> representing a set of permissions.</returns>
    Task<PermissionsSet?> GetRepositoryDisplayPermissionsAsync(string organization, Guid projectId, Guid repositoryId,
        Guid teamsFoundationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the release displaypermissions
    /// </summary>
    /// <param name="organization">The organization the repository belongs to</param>
    /// <param name="projectId">The project the repository belongs to</param>
    /// <param name="pipelineId">The ID of the pipeline</param>
    /// <param name="pipelinePath">The path of the pipeline</param>
    /// <param name="teamsFoundationId">The TeamFoundationID of passed along with the request</param>
    /// <param name="cancellationToken">Cancels the API call if necessary.</param>
    /// <returns>Nullable <see cref="PermissionsSet"/> representing a set of permissions.</returns>
    Task<PermissionsSet?> GetReleaseDefinitionDisplayPermissionsAsync(string organization, Guid projectId,
    string pipelineId, string pipelinePath, Guid teamsFoundationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the displaypermissions for a BuildDefinition
    /// </summary>
    /// <param name="organization">The organization the builddefinition belongs to</param>
    /// <param name="projectId">The project the builddefinition belongs to</param>
    /// <param name="pipelineId">The ID of the builddefinition</param>
    /// <param name="pipelinePath">The path of the builddefinition</param>
    /// <param name="teamsFoundationId">The TeamFoundationID of passed along with the request</param>
    /// <param name="cancellationToken">Cancels the API call if necessary.</param>
    /// <returns>Nullable <see cref="PermissionsSet"/> representing a set of permissions.</returns>
    Task<PermissionsSet?> GetBuildDefinitionDisplayPermissionsAsync(string organization, Guid projectId,
    string pipelineId, string pipelinePath, Guid teamsFoundationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the displaypermissions for a ApplicationGroup
    /// </summary>
    /// <param name="organization">The organization the ApplicationGroup belongs to</param>
    /// <param name="projectId">The project the ApplicationGroup belongs to</param>
    /// <param name="applicationGroupId">The ID of the applicationGroup. It is passed as teamsFoundationId in the request</param>
    /// <param name="cancellationToken">Cancels the API call if necessary.</param>
    /// <returns>Nullable <see cref="PermissionsSet"/> representing a set of permissions.</returns>
    Task<PermissionsSet?> GetApplicationGroupDisplayPermissionsAsync(string organization, Guid projectId, Guid applicationGroupId, CancellationToken cancellationToken = default);
}