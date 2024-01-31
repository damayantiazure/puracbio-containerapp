#nullable enable

using Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers;
using Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers.PermissionFlags;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Infrastructure.Constants;

namespace Rabobank.Compliancy.Infrastructure.Permissions.Context;

public class GitRepoPermissionedContext : PermissionedContext<GitRepo>
{
    private readonly List<string> _nativeAzureDevOpsSecurityDisplayNames = new()
    {
        NativeAzureDevOpsSecurityDisplayNames.ProjectAdministrators,
        NativeAzureDevOpsSecurityDisplayNames.BuildAdministrators,
        NativeAzureDevOpsSecurityDisplayNames.Contributors
    };

    public GitRepoPermissionedContext(IPermissionsHandler<GitRepo> handler, GitRepo resource) : base(handler, resource)
    {
    }

    public override Guid GetSecurityNamespace() => SecurityNamespaces.GitRepo;

    public override IList<int> GetPermissionBitsInScope()
    {
        return new List<int> {
            (int)GitRepositoryPermissionBits.ManagePermissions,
            (int)GitRepositoryPermissionBits.DeleteRepository };
    }

    public override IEnumerable<string> GetNativeAzureDevOpsSecurityDisplayNames()
    {
        return _nativeAzureDevOpsSecurityDisplayNames;
    }

    public override List<string> GetRetentionQuery(TimeSpan retentionPeriodInDays)
    {
        var auditDeploymentQuery = $@"{LogAnalyticsLogNamesConstants.AuditDeploymentLogTable}
                | where CompletedOn_t > ago({retentionPeriodInDays.TotalDays:F0}d)
                | where RepoUrls_s has '{Resource.Project.Organization}/{Resource.Project.Id}/_git/{Resource?.Id}'
                | sort by CompletedOn_t desc
                | limit 1
                | project CompletedOn_t, CiName_s, RunUrl_s";

        return new List<string> { auditDeploymentQuery };
    }
}