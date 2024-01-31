namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;

public static class AuditLogNames
{
    public const string AuditLogName = "audit_deployment_log";
    public const string AuditErrorLogName = "audit_logging_error_log";
    public const string AuditHookFailureLogName = "audit_logging_hook_failure_log";
    public const string AuditLogTimeField = "createdDate";
    public const string AuditPullRequestApproversLogName = "audit_pull_request_approvers_log";
    public const string AuditPoisonMessagesLogName = "audit_poison_messages_log";
    public const string DecoratorErrorLog = "decorator_error_log";
}