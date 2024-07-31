using Microsoft.Extensions.Caching.Memory;
using Rabobank.Compliancy.Core.Rules.Exceptions;
using Rabobank.Compliancy.Core.Rules.Helpers;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Constants;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using static Rabobank.Compliancy.Infra.AzdoClient.Requests.SecurityManagement;
using Bits = Rabobank.Compliancy.Infra.AzdoClient.Permissions.Bits;
using Response = Rabobank.Compliancy.Infra.AzdoClient.Response;

namespace Rabobank.Compliancy.Core.Rules.Rules;

public class NobodyCanManageEnvironmentGatesAndDeploy : ReconcilableBuildPipelineRule, IYamlReleasePipelineRule, IReconcile
{
    private const string AdministratorRole = "Administrator";
    private const string UserRole = "User";

    private readonly IAzdoRestClient _client;
    private readonly IMemoryCache _cache;
    private readonly IYamlEnvironmentHelper _yamlEnvironmentHelper;

    public NobodyCanManageEnvironmentGatesAndDeploy(IAzdoRestClient client, IMemoryCache cache, IYamlEnvironmentHelper yamlEnvironmentHelper)
        : base(client)
    {
        _client = client;
        _cache = cache;
        _yamlEnvironmentHelper = yamlEnvironmentHelper;
    }
    [ExcludeFromCodeCoverage]
    string IRule.Name => RuleNames.NobodyCanManageEnvironmentGatesAndDeploy;
    [ExcludeFromCodeCoverage]
    string IReconcile.Name => RuleNames.NobodyCanManageEnvironmentGatesAndDeploy;
    [ExcludeFromCodeCoverage]
    string IRule.Description => "Nobody can both manage environment checks and start deployments";
    [ExcludeFromCodeCoverage]
    string IRule.Link => "https://confluence.dev.rabobank.nl/x/hhwqEg";
    [ExcludeFromCodeCoverage]
    BluePrintPrinciple[] IRule.Principles => new[] { BluePrintPrinciples.FourEyes };
    [ExcludeFromCodeCoverage]
    string[] IReconcile.Impact => new[]
    {
        "If the Production Environment Owner (PEO) group does not exist, this group will be created with the " +
        "pipeline permission 'Queue Builds' and repository permissions 'Contribute' and 'Force Push' set to Deny " +
        "and the role on the environment set to 'Administrator'. " +
        "Please note that user(s) should be added manually to the PEO group",
        "For all other security groups/users that can both manage environments AND start deployment, " +
        "the role on the environment is set from 'Administrator' to 'User'"
    };

    public async override Task<bool> EvaluateAsync(
        string organization, string projectId, Response.BuildDefinition buildPipeline)
    {
        try
        {
            ValidateInput(organization, projectId, buildPipeline);

            var prodEnvironments = await _yamlEnvironmentHelper
                .GetProdEnvironmentsAsync(organization, projectId, buildPipeline);

            if (!prodEnvironments.Any())
            {
                return false;
            }

            var results = await Task.WhenAll(prodEnvironments
                .Select(async e => await EvaluateEnvironmentAsync(organization, projectId, buildPipeline, e.Id)));

            return results.All(r => r);
        }
        catch (Exception e) when (
            e is InvalidClassicPipelineException ||
            e is InvalidYamlPipelineException ||
            e is EnvironmentNotFoundException ||
            e is InvalidEnvironmentException)
        {
            return false;
        }
    }

    public async override Task ReconcileAsync(string organization, string projectId, string itemId)
    {
        ValidateInput(organization, projectId, itemId);

        var buildPipeline = await _client.GetAsync(Builds.BuildDefinition(projectId, itemId), organization);

        var peoGroup = await CreatePeoGroupIfNotExists(organization, projectId);

        var prodEnvironments = await _yamlEnvironmentHelper
            .GetProdEnvironmentsAsync(organization, projectId, buildPipeline);

        await Task.WhenAll(prodEnvironments
            .Select(async e => await ReconcileEnvironmentAsync(
                organization, projectId, buildPipeline, e.Id, peoGroup)));
    }

    private static void ValidateInput(string organization, string projectId,
        Response.BuildDefinition buildPipeline)
    {
        if (organization == null)
        {
            throw new ArgumentNullException(nameof(organization));
        }

        if (projectId == null)
        {
            throw new ArgumentNullException(nameof(projectId));
        }

        if (buildPipeline == null)
        {
            throw new ArgumentNullException(nameof(buildPipeline));
        }
    }

    private static void ValidateInput(string organization, string projectId, string itemId)
    {
        if (organization == null)
        {
            throw new ArgumentNullException(nameof(organization));
        }

        if (projectId == null)
        {
            throw new ArgumentNullException(nameof(projectId));
        }

        if (itemId == null)
        {
            throw new ArgumentNullException(nameof(itemId));
        }
    }

    private async Task<bool> EvaluateEnvironmentAsync(string organization, string projectId,
        Response.BuildDefinition buildPipeline, int environmentId)
    {
        var adminGroupIds = await GetEnvironmentAdminGroupsAsync(organization, projectId, environmentId);

        if (!adminGroupIds.Any())
        {
            return true;
        }

        return await HasCorrectPermissionsAsync(organization, projectId, buildPipeline, adminGroupIds);
    }

    private async Task<Guid[]> GetEnvironmentAdminGroupsAsync(
        string organization, string projectId, int environmentId)
    {
        var groups = await _client.GetAsync(Environments.Security(projectId, environmentId), organization);
        return groups
            .Where(g => g.Role.Name == AdministratorRole && g.Access != "inherited")
            .Select(g => g.Identity.Id)
            .ToArray();
    }

    private async Task<bool> HasCorrectPermissionsAsync(string organization, string projectId,
        Response.BuildDefinition buildPipeline, Guid[] adminGroupIds)
    {
        bool hasCorrectPipelinePermissions = await ManagePermissions
            .SetSecurityContextToSpecificBuildPipeline(_client, _cache, organization, projectId, buildPipeline.Id, buildPipeline.Path)
            .SetPermissionGroupTeamFoundationIdentifiers(adminGroupIds)
            .SetPermissionsToBeInScope(Bits.BuildDefinitionBits.QueueBuilds)
            .SetPermissionLevelIdsThatAreOkToHave(PermissionLevelId.Deny, PermissionLevelId.DenyInherited)
            .ValidateAsync();

        bool hasCorrectRepoPermissions = await ManagePermissions
            .SetSecurityContextToSpecificRepository(_client, _cache, organization, projectId, buildPipeline.Repository.Id)
            .SetPermissionGroupTeamFoundationIdentifiers(adminGroupIds)
            .SetPermissionsToBeInScope(Bits.RepositoryBits.Contribute, Bits.RepositoryBits.ForcePush)
            .SetPermissionLevelIdsThatAreOkToHave(PermissionLevelId.Deny, PermissionLevelId.DenyInherited)
            .ValidateAsync();

        return hasCorrectPipelinePermissions && hasCorrectRepoPermissions;
    }

    private async Task<string> CreatePeoGroupIfNotExists(string organization, string projectId)
    {
        var groups = await _client.GetAsync(ApplicationGroup.ApplicationGroups(projectId), organization);
        var peoGroup = groups.Identities
            .FirstOrDefault(g => g.FriendlyDisplayName == AzureDevOpsGroups.ProductionEnvironmentOwners);

        if (peoGroup != null)
        {
            return peoGroup.TeamFoundationId;
        }

        return (await _client.PostAsync(ManageGroup(projectId),
                new ManageGroupData { Name = AzureDevOpsGroups.ProductionEnvironmentOwners }, organization))
            .TeamFoundationId;
    }

    private async Task ReconcileEnvironmentAsync(string organization, string projectId,
        Response.BuildDefinition buildPipeline, int environmentId, string peoGroup)
    {
        var adminGroupIds = await GetEnvironmentAdminGroupsAsync(organization, projectId, environmentId);

        if (!PeoGroupExistsOnEnvironment(peoGroup, adminGroupIds))
        {
            await _client.PutAsync(Environments.UpdateSecurity(projectId, environmentId),
                Environments.CreateUpdateSecurityBody(peoGroup, AdministratorRole), organization);
            adminGroupIds.ToList().Add(new Guid(peoGroup));
        }

        await Task.WhenAll(adminGroupIds
            .Select(async g => await ReconcileGroupAsync(
                organization, projectId, buildPipeline, environmentId, peoGroup, g)));
    }

    private static bool PeoGroupExistsOnEnvironment(string peoGroup, Guid[] adminGroupIds) =>
        adminGroupIds
            .Select(g => g.ToString())
            .Contains(peoGroup);

    private async Task ReconcileGroupAsync(string organization, string projectId,
        Response.BuildDefinition buildPipeline, int environmentId, string peoGroup, Guid groupId)
    {
        if (groupId.ToString() == peoGroup)
        {
            await ReconcilePeoGroupAsync(organization, projectId, buildPipeline, groupId);
            return;
        }

        var hasCorrectPermissions = await HasCorrectPermissionsAsync(
            organization, projectId, buildPipeline, new[] { groupId });
        if (hasCorrectPermissions)
        {
            return;
        }

        await _client.PutAsync(Environments.UpdateSecurity(projectId, environmentId),
            Environments.CreateUpdateSecurityBody(groupId.ToString(), UserRole), organization);
    }

    private async Task ReconcilePeoGroupAsync(string organization, string projectId,
        Response.BuildDefinition buildPipeline, Guid groupId)
    {
        await ManagePermissions
            .SetSecurityContextToSpecificBuildPipeline(_client, _cache, organization, projectId, buildPipeline.Id, buildPipeline.Path)
            .SetPermissionGroupTeamFoundationIdentifiers(groupId)
            .SetPermissionsToBeInScope(Bits.BuildDefinitionBits.QueueBuilds)
            .SetPermissionLevelIdsThatAreOkToHave(PermissionLevelId.Deny, PermissionLevelId.DenyInherited)
            .UpdatePermissionsInScopeForGroupsInScopeAsync(PermissionLevelId.Deny);

        await ManagePermissions
            .SetSecurityContextToSpecificRepository(_client, _cache, organization, projectId, buildPipeline.Repository.Id)
            .SetPermissionGroupTeamFoundationIdentifiers(groupId)
            .SetPermissionsToBeInScope(Bits.RepositoryBits.Contribute, Bits.RepositoryBits.ForcePush)
            .SetPermissionLevelIdsThatAreOkToHave(PermissionLevelId.Deny, PermissionLevelId.DenyInherited)
            .UpdatePermissionsInScopeForGroupsInScopeAsync(PermissionLevelId.Deny);
    }
}