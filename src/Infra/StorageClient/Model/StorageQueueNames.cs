#nullable enable

namespace Rabobank.Compliancy.Infra.StorageClient.Model;

public static class StorageQueueNames
{
    public const string AuditClassicReleaseQueueName = "auditreleasedeployment";
    public const string AuditYamlReleaseQueueName = "auditmultistagedeployment";
    public const string AuditPullRequestApproversQueueName = "auditpullrequestapprovers";
    public const string DeviationReportQueueName = "deviationreportlogrecords";
}