using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;

/// <inheritdoc/>
public class PermissionRepository : IPermissionRepository
{
    private readonly IDevHttpClientCallHandler _httpClientCallHandler;

    public PermissionRepository(IDevHttpClientCallHandler httpClientCallHandler) =>
        _httpClientCallHandler = httpClientCallHandler;

    /// <inheritdoc/>
    public async Task<MembersGroupResponse?> AddMembersToGroupsAsync(string organization, Guid projectId, IEnumerable<Guid> users, IEnumerable<Guid> groups, CancellationToken cancellationToken = default)
    {
        var addMemberData = new AddMemberData(users, groups);
        return await new AddMemberRequest(organization, projectId, addMemberData, _httpClientCallHandler).ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Group?> CreateManageGroupAsync(string organization, Guid projectId, string? groupName, CancellationToken cancellationToken = default)
    {
        return await new CreateManagedGroupRequest(organization, projectId, groupName, _httpClientCallHandler).ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ProjectGroup?> GetProjectGroupAsync(string organization, Guid projectId, CancellationToken cancellationToken = default)
    {
        return await new GetProjectGroupsRequest(organization, projectId, _httpClientCallHandler).ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PermissionsProjectId?> GetPermissionsUserOrGroupAsync(string organization, Guid projectId, Guid id, CancellationToken cancellationToken = default)
    {
        return await new GetUserPermissionsRequest(organization, projectId, id, _httpClientCallHandler).ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PermissionsSet?> UpdatePermissionGroupAsync(string organization, Guid projectId, UpdatePermissionBodyContent updatePermissionBodyContent, CancellationToken cancellationToken = default)
    {
        return await new UpdatePermissionRequest(organization, projectId, updatePermissionBodyContent,
            _httpClientCallHandler).ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PermissionsSet?> GetRepositoryDisplayPermissionsAsync(string organization, Guid projectId, Guid repositoryId, Guid teamsFoundationId, CancellationToken cancellationToken = default)
    {
        return await new GetRepositoryDisplayPermissionsRequest(organization, projectId, repositoryId, teamsFoundationId, _httpClientCallHandler)
            .ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PermissionsSet?> GetReleaseDefinitionDisplayPermissionsAsync(string organization, Guid projectId, string pipelineId, string pipelinePath, Guid teamsFoundationId, CancellationToken cancellationToken = default)
    {
        return await new GetReleaseDefinitionDisplayPermissionRequest(organization, projectId, pipelineId, pipelinePath, teamsFoundationId, _httpClientCallHandler)
            .ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PermissionsSet?> GetBuildDefinitionDisplayPermissionsAsync(string organization, Guid projectId, string pipelineId, string pipelinePath, Guid teamsFoundationId, CancellationToken cancellationToken = default)
    {
        return await new GetBuildDefinitionDisplayPermissionRequest(organization, projectId, pipelineId, pipelinePath, teamsFoundationId, _httpClientCallHandler)
            .ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PermissionsSet?> GetApplicationGroupDisplayPermissionsAsync(string organization, Guid projectId, Guid applicationGroupId, CancellationToken cancellationToken = default)
    {
        return await new GetApplicationGroupDisplayPermissionsRequest(organization, projectId, applicationGroupId, _httpClientCallHandler)
            .ExecuteAsync(cancellationToken: cancellationToken);
    }
}