using Microsoft.Extensions.Caching.Memory;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Bits;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Constants;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants.AzureDevOpsGroups;
using Response = Rabobank.Compliancy.Infra.AzdoClient.Response;

namespace Rabobank.Compliancy.Core.Rules.Rules;

public class NobodyCanDeleteBuilds : ReconcilableBuildPipelineRule, IYamlReleasePipelineRule, IReconcile
{
    private readonly IAzdoRestClient _client;

    private readonly IMemoryCache _cache;

    public NobodyCanDeleteBuilds(IAzdoRestClient client, IMemoryCache cache) : base(client)
    {
        _client = client;
        _cache = cache;
    }
    [ExcludeFromCodeCoverage]
    string IRule.Name => RuleNames.NobodyCanDeleteBuilds;
    [ExcludeFromCodeCoverage]
    string IReconcile.Name => RuleNames.NobodyCanDeleteBuilds;
    [ExcludeFromCodeCoverage]
    string IRule.Description => "Nobody can delete builds";
    [ExcludeFromCodeCoverage]
    string IRule.Link => "https://confluence.dev.rabobank.nl/x/V48AD";
    [ExcludeFromCodeCoverage]
    BluePrintPrinciple[] IRule.Principles =>
        new[] { BluePrintPrinciples.Auditability };

    [ExcludeFromCodeCoverage]
    string[] IReconcile.Impact => new[]
    {
        "For all security groups the 'Delete Builds' permission is set to Deny",
        "For all security groups the 'Destroy Builds' permission is set to Deny",
        "For all security groups the 'Delete Build Definitions' permission is set to Deny",
        "For all security groups the 'Administer Build Permissions' permission is set to Deny"
    };

    public override Task<bool> EvaluateAsync(
        string organization, string projectId, Response.BuildDefinition buildPipeline)
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

        return EvaluateInternalAsync(organization, projectId, buildPipeline);
    }

    private async Task<bool> EvaluateInternalAsync(string organization, string projectId, Response.BuildDefinition buildPipeline)
    {
        return await Permissions(organization, projectId, buildPipeline.Id, buildPipeline.Path).ValidateAsync();
    }

    public override Task ReconcileAsync(string organization, string projectId, string itemId)
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

        return ReconcileInternalAsync(organization, projectId, itemId);
    }

    private async Task ReconcileInternalAsync(string organization, string projectId, string itemId)
    {
        var buildPipeline = await _client.GetAsync(Builds.BuildDefinition(projectId, itemId),
            organization);

        await Permissions(organization, projectId, itemId, buildPipeline.Path)
            .UpdatePermissionsInScopeForGroupsInScopeAsync(PermissionLevelId.Deny);
    }

    private ManagePermissions Permissions(
        string organization, string projectId, string itemId, string itemPath) =>
        ManagePermissions
            .SetSecurityContextToSpecificBuildPipeline(_client, _cache, organization, projectId, itemId, itemPath)
            .SetApplicationGroupDisplayNamesToIgnore(
                ProjectCollectionAdministrators,
                ProjectCollectionBuildAdministrators)
            .SetPermissionsToBeInScope(BuildDefinitionBits.DeleteBuilds, BuildDefinitionBits.DestroyBuilds, BuildDefinitionBits.DeleteBuildDefinition, BuildDefinitionBits.AdministerBuildPermissions)
            .SetPermissionLevelIdsThatAreOkToHave(PermissionLevelId.NotSet, PermissionLevelId.Deny, PermissionLevelId.DenyInherited);
}