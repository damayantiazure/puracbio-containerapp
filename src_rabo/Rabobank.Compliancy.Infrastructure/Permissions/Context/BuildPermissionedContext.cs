#nullable enable

using Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers;
using Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers.PermissionFlags;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Infrastructure.AzureDevOps;
using Rabobank.Compliancy.Infrastructure.Constants;

namespace Rabobank.Compliancy.Infrastructure.Permissions.Context;

public class BuildPermissionedContext : PermissionedContext<AzdoBuildDefinitionPipeline>
{
    private readonly List<string> _nativeAzureDevOpsSecurityDisplayNames = new()
    {
        NativeAzureDevOpsSecurityDisplayNames.ProjectAdministrators,
        NativeAzureDevOpsSecurityDisplayNames.BuildAdministrators,
        NativeAzureDevOpsSecurityDisplayNames.Contributors
    };

    public BuildPermissionedContext(IPermissionsHandler<AzdoBuildDefinitionPipeline> handler, AzdoBuildDefinitionPipeline resource) : base(handler, resource)
    {
    }

    public override Guid GetSecurityNamespace()
    {
        return SecurityNamespaces.Build;
    }

    public override IList<int> GetPermissionBitsInScope()
    {
        return new List<int> {
            (int)BuildPermissionBits.DeleteBuilds,
            (int)BuildPermissionBits.DestroyBuilds,
            (int)BuildPermissionBits.DeleteBuildDefinition,
            (int)BuildPermissionBits.AdministerBuildPermissions };
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

        var auditDeploymentLogByPipeline = $@"{LogAnalyticsLogNamesConstants.AuditDeploymentLogTable}
                | where CompletedOn_t > ago({retentionPeriodInDays.TotalDays:F0}d)
                | where Organization_s == '{organization}' or RunUrl_s contains '/{organization}/'
                | where ProjectId_g == '{projectId}' or RunUrl_s contains '{projectId}'
                | where PipelineId_s == {id}
                | sort by CompletedOn_t desc
                | limit 1
                | project CompletedOn_t, CiName_s, RunUrl_s";

        var auditDeploymentLogByBuildDefinition = $@"{LogAnalyticsLogNamesConstants.AuditDeploymentLogTable}
                | where CompletedOn_t > ago({retentionPeriodInDays.TotalDays:F0}d)
                | where BuildUrls_s has '{organization}/{projectId}/_build/definition?definitionId={id}'
                | sort by CompletedOn_t desc
                | limit 1
                | project CompletedOn_t, CiName_s, RunUrl_s";

        return new List<string> { auditDeploymentLogByPipeline, auditDeploymentLogByBuildDefinition };
    }
}