using Microsoft.Extensions.Caching.Memory;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Extensions;
using Rabobank.Compliancy.Infra.AzdoClient.Model;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Bits;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Constants;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.StorageClient;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using static Rabobank.Compliancy.Infra.AzdoClient.Requests.SecurityManagement;
using Response = Rabobank.Compliancy.Infra.AzdoClient.Response;

namespace Rabobank.Compliancy.Core.Rules.Rules;

public class NobodyCanManagePipelineGatesAndDeploy : ReconcilableClassicReleasePipelineRule, IClassicReleasePipelineRule, IReconcile
{
    private readonly IAzdoRestClient _client;
    private readonly IMemoryCache _cache;
    private readonly IPipelineRegistrationResolver _productionItemsResolver;

    public NobodyCanManagePipelineGatesAndDeploy(IAzdoRestClient client, IMemoryCache cache, IPipelineRegistrationResolver productionItemsResolver)
        : base(client)
    {
        _client = client;
        _cache = cache;
        _productionItemsResolver = productionItemsResolver;
    }
    [ExcludeFromCodeCoverage]
    string IRule.Name => RuleNames.NobodyCanManagePipelineGatesAndDeploy;
    [ExcludeFromCodeCoverage]
    string IReconcile.Name => RuleNames.NobodyCanManagePipelineGatesAndDeploy;
    [ExcludeFromCodeCoverage]
    string IRule.Description => "Nobody can both manage pipeline gates and start deployments";
    [ExcludeFromCodeCoverage]
    string IRule.Link => "https://confluence.dev.rabobank.nl/x/IdKEEQ";
    [ExcludeFromCodeCoverage] BluePrintPrinciple[] IRule.Principles => new[] { BluePrintPrinciples.FourEyes };
    [ExcludeFromCodeCoverage]
    string[] IReconcile.Impact => new[]
    {
        "If the Production Environment Owner group does not exist, this group will be created with the " +
        "'Create Releases' and 'Manage Deployments' permissions set to Deny and the 'Manage Release Approvers' permission set to Allow",
        "Please note that user(s) should be added manually to the Production Environment Owner group",
        "For all other security groups where the 'Create Releases' or 'Manage Deployments' permission is set to Allow, " +
        "the 'Manage release approvers' permission is set to NotSet on the registered production stage"
    };

    private static IEnumerable<int> AllowedPermissionsManageApprovals => new[]
    {
        PermissionLevelId.NotSet,
        PermissionLevelId.Deny,
        PermissionLevelId.DenyInherited
    };
    private static IEnumerable<int> AllowedPermissionsStartDeployments => new[]
    {
        PermissionLevelId.Deny,
        PermissionLevelId.DenyInherited
    };
    private static IEnumerable<string> IgnoredIdentitiesDisplayNames => new[]
    {
        Constants.AzureDevOpsGroups.ProjectCollectionAdministrators
    };

    public async override Task<bool> EvaluateAsync(string organization, string projectId, Response.ReleaseDefinition releasePipeline)
    {
        ValidateInput(organization, projectId, releasePipeline);

        var productionStageIds = await _productionItemsResolver.ResolveProductionStagesAsync(
            organization, projectId, releasePipeline.Id);

        if (!productionStageIds.Any())
        {
            return false;
        }

        var releasePipelineGroups = await GetGroupsAsync(organization, projectId, releasePipeline);

        foreach (var stageId in productionStageIds)
        {
            var releasePipelineStageGroups =
                await GetGroupsAsync(organization, projectId, releasePipeline, stageId);

            var results = await Task.WhenAll(releasePipelineStageGroups
                .Select(async g => await ValidateGroupPermissionsAsync(
                    organization, projectId, releasePipeline, stageId, g, releasePipelineGroups)));

            if (results.Any(x => !x))
            {
                return false;
            }
        }
        return true;
    }

    public async override Task ReconcileAsync(string organization, string projectId, string itemId)
    {
        ValidateInput(organization, projectId, itemId);

        var productionStageIds = await _productionItemsResolver.ResolveProductionStagesAsync(
            organization, projectId, itemId);

        var releasePipeline =
            await _client.GetAsync(ReleaseManagement.Definition(projectId, itemId), organization);
        var projectGroups = await GetGroupsAsync(organization, projectId);
        var releasePipelineGroups = await GetGroupsAsync(organization, projectId, releasePipeline);

        await CreatePeoGroupIfNotExistsAsync(
            organization, projectId, projectGroups, releasePipelineGroups, releasePipeline);

        foreach (var stageId in productionStageIds)
        {
            var releasePipelineStageGroups = await GetGroupsAsync(
                organization, projectId, releasePipeline, stageId);

            await Task.WhenAll(releasePipelineStageGroups
                .Select(async g => await ReconcileGroupPermissionsAsync(
                    organization, projectId, releasePipeline, stageId, g, releasePipelineGroups)));
        }
    }

    private static void ValidateInput(string organization, string projectId,
        Response.ReleaseDefinition releasePipeline)
    {
        if (organization == null)
        {
            throw new ArgumentNullException(nameof(organization));
        }

        if (projectId == null)
        {
            throw new ArgumentNullException(nameof(projectId));
        }

        if (releasePipeline == null)
        {
            throw new ArgumentNullException(nameof(releasePipeline));
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

    private async Task<bool> ValidateGroupPermissionsAsync(string organization, string projectId,
        Response.ReleaseDefinition releasePipeline, string stageId, Response.ApplicationGroup group,
        IEnumerable<Response.ApplicationGroup> releasePipelineGroups)
    {
        var permissionSetTokenStage =
            ExtractToken(projectId, releasePipeline.Id, releasePipeline.Path, stageId);
        var permissionSetIdStage = await GetPermissionSetIdAsync(
            organization, projectId, group, permissionSetTokenStage);

        if (HasValidPermission(permissionSetIdStage, AllowedPermissionsManageApprovals,
                ReleasePipelineStageBits.ManageApprovals))
        {
            return true;
        }

        if (!HasValidPermission(permissionSetIdStage, AllowedPermissionsStartDeployments,
                ReleasePipelineStageBits.ManageDeployments))
        {
            return false;
        }

        var pipelineGroup = releasePipelineGroups
            .FirstOrDefault(g => g.FriendlyDisplayName == group.FriendlyDisplayName);
        if (pipelineGroup == null)
        {
            return true;
        }

        var permissionSetTokenPipeline = ExtractToken(projectId, releasePipeline.Id, releasePipeline.Path);
        var permissionSetIdPipeline = await GetPermissionSetIdAsync(
            organization, projectId, pipelineGroup, permissionSetTokenPipeline);
        if (!HasValidPermission(permissionSetIdPipeline, AllowedPermissionsStartDeployments,
                ReleaseDefinitionBits.CreateReleases))
        {
            return false;
        }

        return true;
    }

    private async Task<IEnumerable<Response.ApplicationGroup>> GetGroupsAsync(string organization,
        string projectId, Response.ReleaseDefinition releasePipeline, string stageId) =>
        GetFilteredIdentities(await _client.GetAsync(ApplicationGroup.ExplicitIdentitiesPipelineStage(
            projectId, SecurityNamespaceIds.Release, releasePipeline.Id, stageId), organization));

    private async Task<IEnumerable<Response.ApplicationGroup>> GetGroupsAsync(
        string organization, string projectId, Response.ReleaseDefinition releasePipeline) =>
        GetFilteredIdentities(await _client.GetAsync(ApplicationGroup.ExplicitIdentitiesPipelines(
            projectId, SecurityNamespaceIds.Release, releasePipeline.Id), organization));

    private async Task<IEnumerable<Response.ApplicationGroup>> GetGroupsAsync(
        string organization, string projectId) =>
        GetFilteredIdentities(await _client.GetAsync(ApplicationGroup.ApplicationGroups(projectId),
            organization));

    private static IEnumerable<Response.ApplicationGroup> GetFilteredIdentities(
        Response.ApplicationGroups applicationGroups) =>
        applicationGroups.Identities
            .Where(g => !IgnoredIdentitiesDisplayNames.Contains(g.FriendlyDisplayName));

    private static string ExtractToken(string projectId, string pipelineId, string path) =>
        path == "\\"
            ? $"{projectId}/{pipelineId}"
            : $"{projectId}{path.Replace("\\", "/", StringComparison.InvariantCulture)}/{pipelineId}";

    private static string ExtractToken(string projectId, string pipelineId, string path, string stageId) =>
        path == "\\"
            ? $"{projectId}/{pipelineId}/Environment/{stageId}"
            : $"{projectId}{path.Replace("\\", "/", StringComparison.InvariantCulture)}/{pipelineId}/Environment/{stageId}";

    private Task<Response.PermissionsSet> GetPermissionSetIdAsync(string organization,
        string projectId, Response.ApplicationGroup group, string permissionSetToken) =>
        _cache.GetOrCreateAsync(_client, Permissions.PermissionsGroupSetIdDefinition(
                projectId, SecurityNamespaceIds.Release, group.TeamFoundationId, permissionSetToken),
            organization);

    private static bool HasValidPermission(
        Response.PermissionsSet permissionSetId, IEnumerable<int> allowedPermissions, int bit)
    {
        var permission = permissionSetId.Permissions.Single(p => p.PermissionBit == bit);
        return allowedPermissions.Contains(permission.PermissionId);
    }

    private async Task CreatePeoGroupIfNotExistsAsync(string organization, string projectId,
        IEnumerable<Response.ApplicationGroup> projectGroups,
        IEnumerable<Response.ApplicationGroup> releasePipelineGroups,
        Response.ReleaseDefinition releasePipeline)
    {
        var peoGroup = projectGroups.FirstOrDefault(IsPeoGroup) ??
                       await CreatePeoGroupAsync(organization, projectId);
        if (!PeoGroupExists(releasePipelineGroups))
        {
            await SetInitialPermissionsPeoGroupAsync(organization, projectId, peoGroup, releasePipeline);
        }
    }

    private static bool IsPeoGroup(Response.ApplicationGroup group) =>
        group.FriendlyDisplayName == Constants.AzureDevOpsGroups.ProductionEnvironmentOwners;

    private Task<Response.ApplicationGroup> CreatePeoGroupAsync(string organization, string projectId) =>
        _client.PostAsync(ManageGroup(projectId),
            new ManageGroupData { Name = Constants.AzureDevOpsGroups.ProductionEnvironmentOwners },
            organization);

    private static bool PeoGroupExists(IEnumerable<Response.ApplicationGroup> groups) =>
        groups.Any(IsPeoGroup);

    private async Task SetInitialPermissionsPeoGroupAsync(string organization, string projectId,
        Response.ApplicationGroup peoGroup, Response.ReleaseDefinition releasePipeline)
    {
        var permissionSetToken = ExtractToken(projectId, releasePipeline.Id, releasePipeline.Path);
        var permissionSetId = await GetPermissionSetIdAsync(
            organization, projectId, peoGroup, permissionSetToken);

        await UpdatePermissionAsync(organization, projectId, peoGroup, permissionSetId,
            ReleaseDefinitionBits.CreateReleases, PermissionLevelId.Deny);
        await UpdatePermissionAsync(organization, projectId, peoGroup, permissionSetId,
            ReleaseDefinitionBits.ManageApprovals, PermissionLevelId.Allow);
        await UpdatePermissionAsync(organization, projectId, peoGroup, permissionSetId,
            ReleaseDefinitionBits.ViewReleasePipeline, PermissionLevelId.Allow);
        await UpdatePermissionAsync(organization, projectId, peoGroup, permissionSetId,
            ReleaseDefinitionBits.EditReleasePipeline, PermissionLevelId.Allow);
        await UpdatePermissionAsync(organization, projectId, peoGroup, permissionSetId,
            ReleaseDefinitionBits.EditReleaseStage, PermissionLevelId.Allow);
    }

    private async Task ReconcileGroupPermissionsAsync(string organization, string projectId,
        Response.ReleaseDefinition releasePipeline, string stageId, Response.ApplicationGroup group,
        IEnumerable<Response.ApplicationGroup> releasePipelineGroups)
    {
        var permissionSetTokenStage =
            ExtractToken(projectId, releasePipeline.Id, releasePipeline.Path, stageId);
        var permissionSetIdStage = await GetPermissionSetIdAsync(
            organization, projectId, group, permissionSetTokenStage);
        if (HasValidPermission(permissionSetIdStage, AllowedPermissionsManageApprovals,
                ReleasePipelineStageBits.ManageApprovals))
        {
            return;
        }

        var pipelineGroup = releasePipelineGroups
            .FirstOrDefault(g => g.FriendlyDisplayName == group.FriendlyDisplayName);
        if (pipelineGroup == null)
        {
            await UpdatePermissionIfNeededAsync(
                organization, projectId, group, null, permissionSetIdStage);
            return;
        }

        var permissionSetTokenPipeline = ExtractToken(projectId, releasePipeline.Id, releasePipeline.Path);
        var permissionSetIdPipeline = await GetPermissionSetIdAsync(
            organization, projectId, group, permissionSetTokenPipeline);
        await UpdatePermissionIfNeededAsync(
            organization, projectId, group, permissionSetIdPipeline, permissionSetIdStage);
    }

    private async Task UpdatePermissionAsync(string organization, string projectId,
        Response.ApplicationGroup applicationGroup, Response.PermissionsSet permissionSet, int permissionBit, int targetPermissionLevel)
    {
        var permission = permissionSet.Permissions.Single(p => p.PermissionBit == permissionBit);
        //If a permission is inherited it can't be set to NotSet, 
        //therefore in this case permissions are set to Deny
        permission.PermissionId =
            permission.PermissionId == PermissionLevelId.AllowInherited &&
            targetPermissionLevel == PermissionLevelId.NotSet
                ? PermissionLevelId.Deny
                : targetPermissionLevel;

        var managePermissionsData = new ManagePermissionsData(
            applicationGroup.TeamFoundationId, permissionSet.DescriptorIdentifier,
            permissionSet.DescriptorIdentityType, permission.PermissionToken, permission);

        await _client.PostAsync(
            Permissions.ManagePermissions(projectId), managePermissionsData.Wrap(), organization);
    }

    private async Task UpdatePermissionIfNeededAsync(string organization, string projectId,
        Response.ApplicationGroup applicationGroup, Response.PermissionsSet permissionSetPipeline,
        Response.PermissionsSet permissionSetStage)
    {
        var isPeoGroup = IsPeoGroup(applicationGroup);
        var hasValidPipelinePermission = permissionSetPipeline == null ||
                                         HasValidPermission(permissionSetPipeline, AllowedPermissionsStartDeployments,
                                             ReleaseDefinitionBits.CreateReleases);
        var hasValidStagePermission =
            HasValidPermission(permissionSetStage, AllowedPermissionsStartDeployments,
                ReleasePipelineStageBits.ManageDeployments);

        if (isPeoGroup && !hasValidPipelinePermission)
        {
            await UpdatePermissionAsync(organization, projectId, applicationGroup, permissionSetPipeline,
                ReleaseDefinitionBits.CreateReleases, PermissionLevelId.Deny);
        }

        if (isPeoGroup && !hasValidStagePermission)
        {
            await UpdatePermissionAsync(organization, projectId, applicationGroup, permissionSetStage,
                ReleasePipelineStageBits.ManageDeployments, PermissionLevelId.Deny);
        }

        if (!isPeoGroup && (!hasValidPipelinePermission || !hasValidStagePermission))
        {
            await UpdatePermissionAsync(organization, projectId, applicationGroup, permissionSetStage,
                ReleasePipelineStageBits.ManageApprovals, PermissionLevelId.NotSet);
        }
    }
}