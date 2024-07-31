#nullable enable

using Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers;
using Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers.PermissionFlags;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Infrastructure.AzureDevOps;
using Rabobank.Compliancy.Infrastructure.Constants;

namespace Rabobank.Compliancy.Infrastructure.Permissions.Context;

public class ReleasePermissionedContext : PermissionedContext<AzdoReleaseDefinitionPipeline>
{
    private readonly List<string> _nativeAzureDevOpsSecurityDisplayNames = new()
    {
        NativeAzureDevOpsSecurityDisplayNames.ProjectAdministrators,
        NativeAzureDevOpsSecurityDisplayNames.ReleaseAdministrators,
        NativeAzureDevOpsSecurityDisplayNames.Contributors
    };

    public ReleasePermissionedContext(IPermissionsHandler<AzdoReleaseDefinitionPipeline> handler, AzdoReleaseDefinitionPipeline resource) : base(handler, resource)
    {
    }

    public override Guid GetSecurityNamespace()
    {
        return SecurityNamespaces.ReleaseManagement;
    }

    public override IList<int> GetPermissionBitsInScope()
    {
        return new List<int>
        {
            (int)ReleaseManagementPermissionBits.DeleteReleaseDefinition,
            (int)ReleaseManagementPermissionBits.AdministerReleasePermissions,
            (int)ReleaseManagementPermissionBits.DeleteReleases
        };
    }

    public override IEnumerable<string> GetNativeAzureDevOpsSecurityDisplayNames()
    {
        return _nativeAzureDevOpsSecurityDisplayNames;
    }

    public override List<string> GetRetentionQuery(TimeSpan retentionPeriodInDays)
    {
        var organization = Resource.Project.Organization;
        var projectId = Resource.Project.Id;
        var id = Resource.Id;

        var auditDeploymentLog = $@"{LogAnalyticsLogNamesConstants.AuditDeploymentLogTable}
                | where CompletedOn_t > ago({retentionPeriodInDays.TotalDays:F0}d)
                | where Organization_s == '{organization}' or RunUrl_s contains '/{organization}/'
                | where ProjectId_g == '{projectId}' or RunUrl_s contains '{projectId}'
                | where PipelineId_s == {id}
                | sort by CompletedOn_t desc
                | limit 1
                | project CompletedOn_t, CiName_s, RunUrl_s";

        return new List<string> { auditDeploymentLog };
    }
}