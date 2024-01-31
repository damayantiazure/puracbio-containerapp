using Microsoft.Extensions.Caching.Memory;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Bits;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Constants;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using Response = Rabobank.Compliancy.Infra.AzdoClient.Response;

namespace Rabobank.Compliancy.Core.Rules.Rules;

public class NobodyCanDeleteTheProject : IProjectRule, IProjectReconcile
{
    private const string RabobankProjectAdministrators = AzureDevOpsGroups.RabobankProjectAdministrators;
    private const string ProjectAdministrators = AzureDevOpsGroups.ProjectAdministrators;
    private readonly IAzdoRestClient _client;
    private readonly IMemoryCache _cache;

    public NobodyCanDeleteTheProject(IAzdoRestClient client, IMemoryCache cache)
    {
        _cache = cache;
        _client = client;
    }
    [ExcludeFromCodeCoverage]
    string IRule.Name => RuleNames.NobodyCanDeleteTheProject;
    [ExcludeFromCodeCoverage]
    string IProjectReconcile.Name => RuleNames.NobodyCanDeleteTheProject;
    [ExcludeFromCodeCoverage]
    public string Description => "Nobody can delete the project";
    [ExcludeFromCodeCoverage]
    public string Link => "https://confluence.dev.rabobank.nl/x/NY8AD";
    [ExcludeFromCodeCoverage]
    BluePrintPrinciple[] IRule.Principles =>
        new[] { BluePrintPrinciples.Auditability };

    [ExcludeFromCodeCoverage]
    string[] IProjectReconcile.Impact => new[]
    {
        "Rabobank Project Administrators group is created and added to Project Administrators",
        "Delete team project permissions of the Rabobank Project Administrators group is set to deny",
        "Members of the Project Administrators are moved to Rabobank Project Administrators",
        "Delete team project permission is set to 'not set' for all other groups"
    };

    public async Task<bool> EvaluateAsync(string organization, string projectId)
    {
        var groups = await _client.GetAsync(ApplicationGroup.ApplicationGroups(projectId), organization);

        var paGroupHasMembers = await PaGroupHasMembersAsync(organization, projectId, groups);
        if (paGroupHasMembers)
        {
            return false;
        }

        var hasCorrectPermissions = await Permissions(organization, projectId).ValidateAsync();
        if (!hasCorrectPermissions)
        {
            return false;
        }

        var rpaGroupExists = groups.Identities
            .Any(g => g.FriendlyDisplayName == RabobankProjectAdministrators);
        if (!rpaGroupExists)
        {
            return true;
        }

        var rpaGroupHasCorrectPermissions = await RpaPermissions(organization, projectId).ValidateAsync();
        return rpaGroupHasCorrectPermissions;
    }

    public async Task<bool> EvaluateAsync(Domain.Compliancy.Project project)
    {
        return await EvaluateAsync(project.Organization, project.Id.ToString());
    }

    public async Task ReconcileAsync(string organization, string projectId)
    {
        var groups = await _client.GetAsync(ApplicationGroup.ApplicationGroups(projectId), organization);
        var paGroupId = groups.Identities.Single(p => p.FriendlyDisplayName == ProjectAdministrators)
            .TeamFoundationId;
        var rpaGroupId = (await CreateRpaGroupIfNotExistsAsync(organization, projectId, groups))
            .TeamFoundationId;

        var membersIds = (await _client.GetAsync(ApplicationGroup.GroupMembers(projectId, paGroupId), organization))
            .Identities
            .Where(x => x.TeamFoundationId != rpaGroupId)
            .Select(x => x.TeamFoundationId)
            .ToList();

        await RemoveMembersFromGroupAsync(organization, projectId, membersIds, paGroupId);
        await AddMembersToGroupAsync(organization, projectId, membersIds, rpaGroupId);
        await AddMembersToGroupAsync(organization, projectId, new[] { rpaGroupId }, paGroupId);

        await Permissions(organization, projectId)
            .UpdatePermissionsInScopeForGroupsInScopeAsync(PermissionLevelId.NotSet);
        await RpaPermissions(organization, projectId)
            .UpdatePermissionsInScopeForGroupsInScopeAsync(PermissionLevelId.Deny);
    }

    private async Task<bool> PaGroupHasMembersAsync(
        string organization, string project, Response.ApplicationGroups groups)
    {
        var groupId = groups
            .Identities
            .Single(p => p.FriendlyDisplayName == ProjectAdministrators)
            .TeamFoundationId;

        var members = (await _client.GetAsync(ApplicationGroup.GroupMembers(project, groupId), organization))
            .Identities;

        return members.Any(m => m.FriendlyDisplayName != RabobankProjectAdministrators);
    }

    private ManagePermissions Permissions(string organization, string project) =>
        ManagePermissions
            .SetSecurityContextToTeamProject(_client, _cache, organization, project)
            .SetApplicationGroupDisplayNamesToIgnore(ProjectAdministrators, RabobankProjectAdministrators)
            .SetPermissionsToBeInScope((ProjectBits.DeleteProject, SecurityNamespaceIds.Project))
            .SetPermissionLevelIdsThatAreOkToHave(PermissionLevelId.Deny, PermissionLevelId.DenyInherited, PermissionLevelId.NotSet);

    private ManagePermissions RpaPermissions(string organization, string project) =>
        ManagePermissions
            .SetSecurityContextToTeamProject(_client, _cache, organization, project)
            .SetApplicationGroupsInScopeByDisplayName(RabobankProjectAdministrators)
            .SetPermissionsToBeInScope((ProjectBits.DeleteProject, SecurityNamespaceIds.Project))
            .SetPermissionLevelIdsThatAreOkToHave(PermissionLevelId.Deny, PermissionLevelId.DenyInherited);

    private async Task<Response.ApplicationGroup> CreateRpaGroupIfNotExistsAsync(
        string organization, string project, Response.ApplicationGroups groups) =>
        groups.Identities.SingleOrDefault(p => p.FriendlyDisplayName == RabobankProjectAdministrators)
        ?? await _client.PostAsync(SecurityManagement.ManageGroup(project),
            new SecurityManagement.ManageGroupData { Name = RabobankProjectAdministrators }, organization);

    private async Task AddMembersToGroupAsync(
        string organization, string project, IList<string> memberIds, string groupId) =>
        await _client.PostAsync(SecurityManagement.AddMember(project),
            new SecurityManagement.AddMemberData(memberIds, new[] { groupId }), organization);

    private async Task RemoveMembersFromGroupAsync(
        string organization, string project, IList<string> memberIds, string groupId) =>
        await _client.PostAsync(SecurityManagement.EditMembership(project),
            new SecurityManagement.RemoveMembersData(memberIds, groupId), organization);

    public async Task<bool> ReconcileAndEvaluateAsync(string organization, string projectId)
    {
        await ReconcileAsync(organization, projectId);
        return await EvaluateAsync(organization, projectId);
    }
}