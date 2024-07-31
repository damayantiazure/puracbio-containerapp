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

public class NobodyCanDeleteReleases : ReconcilableClassicReleasePipelineRule, IClassicReleasePipelineRule, IReconcile
{
    private readonly IAzdoRestClient _client;
    private readonly IMemoryCache _cache;

    public NobodyCanDeleteReleases(IAzdoRestClient client, IMemoryCache cache) : base(client)
    {
        _client = client;
        _cache = cache;
    }
    [ExcludeFromCodeCoverage]
    string IRule.Name => RuleNames.NobodyCanDeleteReleases;
    [ExcludeFromCodeCoverage]
    string IReconcile.Name => RuleNames.NobodyCanDeleteReleases;

    [ExcludeFromCodeCoverage]
    string IRule.Description => "Nobody can delete releases";
    [ExcludeFromCodeCoverage]
    string IRule.Link => "https://confluence.dev.rabobank.nl/x/9I8AD";
    [ExcludeFromCodeCoverage]
    BluePrintPrinciple[] IRule.Principles =>
        new[] { BluePrintPrinciples.Auditability };

    [ExcludeFromCodeCoverage]
    string[] IReconcile.Impact => new[]
    {
        "For all security groups the 'Delete Releases' permission is set to Deny",
        "For all security groups the 'Delete Release Pipeline' permission is set to Deny",
        "For all security groups the 'Administer Release Permissions' permission is set to Deny"
    };

    public override Task<bool> EvaluateAsync(
        string organization, string projectId, Response.ReleaseDefinition releasePipeline)
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

        return EvaluateInternalAsync(organization, projectId, releasePipeline);
    }

    private async Task<bool> EvaluateInternalAsync(string organization, string projectId, Response.ReleaseDefinition releasePipeline)
    {
        return await Permissions(organization, projectId, releasePipeline.Id, releasePipeline.Path)
            .ValidateAsync();
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
        var releasePipeline = await _client.GetAsync(ReleaseManagement.Definition(projectId, itemId),
            organization);

        await Permissions(organization, projectId, itemId, releasePipeline.Path)
            .UpdatePermissionsInScopeForGroupsInScopeAsync(PermissionLevelId.Deny);
    }

    private ManagePermissions Permissions(
        string organization, string projectId, string itemId, string itemPath) =>
        ManagePermissions
            .SetSecurityContextToSpecificReleasePipeline(_client, _cache, organization, projectId, itemId, itemPath)
            .SetPermissionsToBeInScope(ReleaseDefinitionBits.DeleteReleasePipelines, ReleaseDefinitionBits.AdministerReleasePermissions, ReleaseDefinitionBits.DeleteReleases)
            .SetPermissionLevelIdsThatAreOkToHave(PermissionLevelId.NotSet, PermissionLevelId.Deny, PermissionLevelId.DenyInherited)
            .SetApplicationGroupDisplayNamesToIgnore(ProjectCollectionAdministrators);
}