using Microsoft.Extensions.Caching.Memory;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Constants;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Infra.AzdoClient.Permissions;

public class ManagePermissions
{
    private readonly ISecurityNamespaceContext _securityContext;

    private Func<(int bit, string namespaceId), bool> _isPermissionInScope = p => true;
    private Func<ApplicationGroup, bool> _isApplicationGroupInScope = applicationGroup => true;
    private IEnumerable<int> _allowedPermissionIds = Enumerable.Empty<int>();
    private IList<string> _applicationGroupDisplayNamesInScope;
    private IList<Guid> _applicationGroupTeamFoundationIdentifiersInScope;

    public ManagePermissions(ISecurityNamespaceContext item) => _securityContext = item;

    /// <summary>
    /// Limits the scope of all <see cref="ManagePermissions"/> operations to a reduced scope defined by the <see cref="ApplicationGroup.FriendlyDisplayName"/> property of the desired <see cref="ApplicationGroup"/>
    /// Cannot be used together with <see cref="SetApplicationGroupDisplayNamesToIgnore(string[])"/>
    /// </summary>
    /// <param name="groupDisplayNamesInScope"></param>
    /// <returns></returns>
    public ManagePermissions SetApplicationGroupsInScopeByDisplayName(params string[] groupDisplayNamesInScope)
    {
        _applicationGroupDisplayNamesInScope = groupDisplayNamesInScope;
        _isApplicationGroupInScope = applicationGroup => groupDisplayNamesInScope.Contains(applicationGroup.FriendlyDisplayName);
        return this;
    }

    /// <summary>
    /// Limits the scope of all <see cref="ManagePermissions"/> operations to a reduced scope defined by the <see cref="ApplicationGroup.TeamFoundationId"/> property of the desired <see cref="ApplicationGroup"/>s.
    /// This will overrule <see cref="SetApplicationGroupsInScopeByDisplayName(string[])"/> and <see cref="SetApplicationGroupDisplayNamesToIgnore(string[])"/>
    /// </summary>
    /// <param name="groupTeamFoundationIds"></param>
    /// <returns></returns>
    public ManagePermissions SetPermissionGroupTeamFoundationIdentifiers(params Guid[] groupTeamFoundationIds)
    {
        _applicationGroupTeamFoundationIdentifiersInScope = groupTeamFoundationIds;
        return this;
    }

    /// <summary>
    /// Sets an Ignore filter for certain <see cref="ApplicationGroup.FriendlyDisplayName"/>s so that these will not be in Scope for any <see cref="ManagePermissions"/> operations.
    /// Cannot be used together with <see cref="SetApplicationGroupsInScopeByDisplayName(string[])"/>
    /// </summary>
    /// <param name="groupDisplayNamesToIgnore"></param>
    /// <returns></returns>
    public ManagePermissions SetApplicationGroupDisplayNamesToIgnore(params string[] groupDisplayNamesToIgnore)
    {
        _isApplicationGroupInScope = applicationGroup => !groupDisplayNamesToIgnore.Contains(applicationGroup.FriendlyDisplayName);
        return this;
    }

    /// <summary>
    /// Limits <see cref="Permission"/>s in scope by their <see cref="Permission.PermissionBit"/>. Ignores SecurityNamespaceId.
    /// Cannot be used together with <see cref="SetPermissionsToBeInScope(ValueTuple{int, string}[])"/>
    /// </summary>
    /// <param name="permissionBits">Set of <see cref="Permission.PermissionBit"/> to be set in scope of <see cref="ManagePermissions"/> operations</param>.
    /// <returns></returns>
    public ManagePermissions SetPermissionsToBeInScope(params int[] permissionBits)
    {
        _isPermissionInScope = p => permissionBits.Any(permissionBit => p.bit == permissionBit);
        return this;
    }

    /// <summary>
    /// Limits <see cref="Permission"/>s in scope by their <see cref="Permission.PermissionBit"/> and SecurityNamespaceId.
    /// Cannot be used together with <see cref="SetPermissionsToBeInScope(int[])"/>
    /// </summary>
    /// <param name="permissionBitAndNamespaceIdsInScope">Set of Combinations of <see cref="Permission.PermissionBit"/> and <see cref="Permission.NamespaceId"/> that the scope should be limited to</param>
    /// <returns></returns>
    public ManagePermissions SetPermissionsToBeInScope(params (int bit, string namespaceId)[] permissionBitAndNamespaceIdsInScope)
    {
        _isPermissionInScope = permissionBitAndNamespaceId => permissionBitAndNamespaceIdsInScope.Any(permissionBit => permissionBit == permissionBitAndNamespaceId);
        return this;
    }

    /// <summary>
    /// Sets the <see cref="PermissionLevelId"/>(s) that are allowed for validation purposes or will be out of scope of permission updates
    /// </summary>
    /// <param name="permissionIds"></param>
    /// <returns></returns>
    public ManagePermissions SetPermissionLevelIdsThatAreOkToHave(params int[] permissionIds)
    {
        _allowedPermissionIds = permissionIds;
        return this;
    }

    /// <summary>
    /// Sets the <see cref="ISecurityNamespaceContext"/> to an instance of <see cref="GenericPipelineSecurityContext"/> for a <see cref="BuildDefinition"/>.
    /// </summary>
    /// <returns></returns>
    public static ManagePermissions SetSecurityContextToSpecificBuildPipeline(IAzdoRestClient client, IMemoryCache cache,
        string organization, string projectId, string pipelineId, string pipelinePath)
    {
        var securitycontext = new GenericPipelineSecurityContext(
            client, cache, organization, projectId, SecurityNamespaceIds.Build, pipelineId, pipelinePath);

        return new ManagePermissions(securitycontext);
    }

    /// <summary>
    /// Sets the <see cref="ISecurityNamespaceContext"/> to an instance of <see cref="GenericPipelineRootFolderSecurityContext"/> for a <see cref="BuildDefinition"/>.
    /// </summary>
    /// <returns></returns>
    public static ManagePermissions SetSecurityContextToBuildPipelineRootFolder(IAzdoRestClient client, IMemoryCache cache,
        string organization, string projectId)
    {
        var securitycontext = new GenericPipelineRootFolderSecurityContext(client, cache, organization, projectId, SecurityNamespaceIds.Build);
        return new ManagePermissions(securitycontext);
    }

    /// <summary>
    /// Sets the <see cref="ISecurityNamespaceContext"/> to an instance of <see cref="GenericPipelineSecurityContext"/> for a <see cref="ReleaseDefinition"/>.
    /// </summary>
    /// <returns></returns>
    public static ManagePermissions SetSecurityContextToSpecificReleasePipeline(IAzdoRestClient client, IMemoryCache cache,
        string organization, string projectId, string pipelineId, string pipelinePath)
    {
        var securitycontext = new GenericPipelineSecurityContext(
            client, cache, organization, projectId, SecurityNamespaceIds.Release, pipelineId, pipelinePath);

        return new ManagePermissions(securitycontext);
    }

    /// <summary>
    /// Sets the <see cref="ISecurityNamespaceContext"/> to an instance of <see cref="RepositorySecurityContext"/>.
    /// </summary>
    /// <returns></returns>
    public static ManagePermissions SetSecurityContextToSpecificRepository(IAzdoRestClient client, IMemoryCache cache,
        string organization, string projectId, string repositoryId)
    {
        var securitycontext = new RepositorySecurityContext(client, cache, organization, projectId, repositoryId);
        return new ManagePermissions(securitycontext);
    }

    /// <summary>
    /// Sets the <see cref="ISecurityNamespaceContext"/> to an instance of <see cref="TeamProjectSecurityContext"/>.
    /// </summary>
    /// <returns></returns>
    public static ManagePermissions SetSecurityContextToTeamProject(IAzdoRestClient client, IMemoryCache cache,
        string organization, string projectId)
    {
        var securitycontext = new TeamProjectSecurityContext(client, cache, organization, projectId);
        return new ManagePermissions(securitycontext);
    }

    /// <summary>
    /// Sets the <see cref="ISecurityNamespaceContext"/> to an instance of <see cref="ClassicReleasePipelineStageSecurityContext"/>.
    /// </summary>
    /// <returns></returns>
    public static ManagePermissions SetSecurityContextToReleasePipelineStage(IAzdoRestClient client, IMemoryCache cache,
        string organization, string projectId, string pipelineId, string stageId, string itemPath)
    {
        var securitycontext = new ClassicReleasePipelineStageSecurityContext(
            client, cache, organization, projectId, pipelineId, stageId, itemPath);

        return new ManagePermissions(securitycontext);
    }

    /// <summary>
    /// Validates the if the <see cref="Permission"/>s in Scope for the <see cref="ApplicationGroup"/>s in Scope are of the allowed <see cref="PermissionLevelId"/>s
    /// </summary>
    /// <returns></returns>
    public async Task<bool> ValidateAsync()
    {
        var groupIds = await GetTeamFoundationIdsOfGroupsInScope();

        var mutex = new SemaphoreSlim(20);
        var permissionSetsForAllGroups = await Task.WhenAll(groupIds.Select(async (applicationGroupId, i) =>
        {
            await mutex.WaitAsync();
            try
            {
                return await _securityContext.GetPermissionSetForApplicationGroup(applicationGroupId);
            }
            finally
            {
                mutex.Release();
            }
        }));

        return permissionSetsForAllGroups.Any() &&
               permissionSetsForAllGroups
                   .SelectMany(permissionSet => permissionSet.Permissions)
                   .Where(permission => _isPermissionInScope((permission.PermissionBit, permission.NamespaceId)))
                   .All(permission => _allowedPermissionIds.Contains(permission.PermissionId));
    }

    /// <summary>
    /// Sets the <see cref="Permission"/>s in Scope to the desired <paramref name="targetPermissionlevel"/> (<see cref="PermissionLevelId"/>) for all <see cref="ApplicationGroup"/>s in Scope.
    /// Providing <see cref="PermissionLevelId.NotSet"/> is treated as wanting to Remove any Allow permissions (Inherited or Explicit).
    /// As such, in case the <paramref name="targetPermissionlevel"/> is <see cref="PermissionLevelId.NotSet"/> and the current Permission Level is <see cref="PermissionLevelId.AllowInherited"/>, it will be set to <see cref="PermissionLevelId.Deny"/> instead.
    /// </summary>
    /// <param name="targetPermissionlevel"></param>
    /// <returns></returns>
    public async Task UpdatePermissionsInScopeForGroupsInScopeAsync(int targetPermissionlevel)
    {
        var groupTeamFoundationIds = await GetTeamFoundationIdsOfGroupsInScope();

        foreach (var groupTeamFoundationId in groupTeamFoundationIds)
        {
            await FilterPermissionsInScopeAndSetTargetPermissionForGroupAsync(groupTeamFoundationId, targetPermissionlevel);
        }
    }

    private async Task<List<string>> GetTeamFoundationIdsOfGroupsInScope()
    {
        if (_applicationGroupTeamFoundationIdentifiersInScope != null)
        {
            return _applicationGroupTeamFoundationIdentifiersInScope
                .Select(i => i.ToString())
                .ToList();
        }

        var allApplicationGroupsWithExplicitPermissions = (await _securityContext.GetAllApplicationGroupsWithExplicitPermissions())?.Identities
            .ToList();

        var InScopeGroupsWithExplicitPermissions = allApplicationGroupsWithExplicitPermissions
            .Where(_isApplicationGroupInScope)
            .ToList();

        if (_applicationGroupDisplayNamesInScope != null)
        {
            var missingGroups = await GetMissingGroups(allApplicationGroupsWithExplicitPermissions);

            InScopeGroupsWithExplicitPermissions.AddRange(missingGroups);
        }

        return InScopeGroupsWithExplicitPermissions.Select(group => group.TeamFoundationId).ToList();
    }

    private async Task<List<ApplicationGroup>> GetMissingGroups(IList<ApplicationGroup> currentlyRetrievedApplicationGroups)
    {
        var applicationGroupsNotYetFound = _applicationGroupDisplayNamesInScope
            .Where(groupNameThatIsInScope => !currentlyRetrievedApplicationGroups
                .Select(g => g.FriendlyDisplayName)
                .Contains(groupNameThatIsInScope));

        return (await Task.WhenAll(applicationGroupsNotYetFound.Select(GetApplicationGroupForDisplayName)))
            .Where(applicationGroup => applicationGroup?.TeamFoundationId != null)
            .ToList();
    }

    private async Task<ApplicationGroup> GetApplicationGroupForDisplayName(string groupDisplayName)
    {
        var allProjectScopedPermissionGroups = (await _securityContext.GetCollectionOfProjectScopedPermissionGroups()).Identities;
        return allProjectScopedPermissionGroups
            .FirstOrDefault(group => group.FriendlyDisplayName == groupDisplayName);
    }

    private async Task FilterPermissionsInScopeAndSetTargetPermissionForGroupAsync(string groupTeamFoundationId, int targetPermissionLevel)
    {
        var permissionSet = await _securityContext.GetPermissionSetForApplicationGroup(groupTeamFoundationId);

        var permissionsInScope = permissionSet.Permissions
            .Where(currentPermission => _isPermissionInScope((currentPermission.PermissionBit, currentPermission.NamespaceId)) &&
                        !_allowedPermissionIds.Contains(currentPermission.PermissionId));

        foreach (var permission in permissionsInScope)
        {
            permission.PermissionId = CanTargetPermissionLevelThrumpCurrent(permission, targetPermissionLevel)
                ? PermissionLevelId.Deny
                : targetPermissionLevel;

            await _securityContext.UpdatePermissionForGroupAsync(groupTeamFoundationId, permissionSet, permission);
        }
    }

    private static bool CanTargetPermissionLevelThrumpCurrent(Permission currentPermission, int targetPermissionLevel)
    {
        return currentPermission.PermissionId == PermissionLevelId.AllowInherited &&
                targetPermissionLevel == PermissionLevelId.NotSet;
    }
}