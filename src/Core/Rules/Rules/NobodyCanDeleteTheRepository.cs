using Microsoft.Extensions.Caching.Memory;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Bits;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Constants;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants.AzureDevOpsGroups;

namespace Rabobank.Compliancy.Core.Rules.Rules;

public sealed class NobodyCanDeleteTheRepository : IRepositoryRule, IReconcile
{
    private readonly IAzdoRestClient _client;

    private readonly IMemoryCache _cache;

    public NobodyCanDeleteTheRepository(IAzdoRestClient client, IMemoryCache cache)
    {
        _client = client;
        _cache = cache;
    }
    [ExcludeFromCodeCoverage]
    string IRule.Name => RuleNames.NobodyCanDeleteTheRepository;
    [ExcludeFromCodeCoverage]
    string IReconcile.Name => RuleNames.NobodyCanDeleteTheRepository;
    [ExcludeFromCodeCoverage]
    string IRule.Description => "Nobody can delete the repository";
    [ExcludeFromCodeCoverage]
    string IRule.Link => "https://confluence.dev.rabobank.nl/x/RI8AD";
    [ExcludeFromCodeCoverage]
    BluePrintPrinciple[] IRule.Principles =>
        new[] { BluePrintPrinciples.Auditability, BluePrintPrinciples.CodeIntegrity };

    [ExcludeFromCodeCoverage]
    string[] IReconcile.Impact => new[]
    {
        "For all security groups the 'Delete Repository' permission is set to Deny",
        "For all security groups the 'Manage Permissions' permission is set to Deny"
    };
    public async Task<bool> EvaluateAsync(GitRepo gitRepo)
    {
        return await EvaluateAsync(gitRepo.Project.Organization, gitRepo.Project.Id.ToString(), gitRepo.Id.ToString());
    }

    public Task<bool> EvaluateAsync(string organization, string projectId, string repositoryId)
    {
        if (organization == null)
        {
            throw new ArgumentNullException(nameof(organization));
        }
        if (projectId == null)
        {
            throw new ArgumentNullException(nameof(projectId));
        }
        if (repositoryId == null)
        {
            throw new ArgumentNullException(nameof(repositoryId));
        }

        return EvaluateInternalAsync(organization, projectId, repositoryId);
    }

    private async Task<bool> EvaluateInternalAsync(string organization, string projectId, string repositoryId)
    {
        return await Permissions(organization, projectId, repositoryId).ValidateAsync();
    }

    public Task ReconcileAsync(string organization, string projectId, string itemId)
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

    private Task ReconcileInternalAsync(string organization, string projectId, string itemId)
    {
        return Permissions(organization, projectId, itemId).UpdatePermissionsInScopeForGroupsInScopeAsync(PermissionLevelId.Deny);
    }

    private ManagePermissions Permissions(string organization, string projectId, string itemId) =>
        ManagePermissions
            .SetSecurityContextToSpecificRepository(_client, _cache, organization, projectId, itemId)
            .SetPermissionsToBeInScope(RepositoryBits.DeleteRepository, RepositoryBits.ManagePermissions)
            .SetPermissionLevelIdsThatAreOkToHave(PermissionLevelId.NotSet, PermissionLevelId.Deny, PermissionLevelId.DenyInherited)
            .SetApplicationGroupDisplayNamesToIgnore(
                ProjectCollectionAdministrators,
                ProjectCollectionServiceAccounts);

    public async Task<bool> ReconcileAndEvaluateAsync(string organization, string projectId, string itemId)
    {
        await ReconcileAsync(organization, projectId, itemId);
        return await EvaluateAsync(organization, projectId, itemId);
    }
}