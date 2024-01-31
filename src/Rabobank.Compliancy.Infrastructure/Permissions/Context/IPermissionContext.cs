namespace Rabobank.Compliancy.Infrastructure.Permissions.Context;
public interface IPermissionContext
{
    Guid GetSecurityNamespace();

    IList<int> GetPermissionBitsInScope();

    List<string> GetRetentionQuery(TimeSpan retentionPeriodInDays);

    IEnumerable<string> GetNativeAzureDevOpsSecurityDisplayNames();
}