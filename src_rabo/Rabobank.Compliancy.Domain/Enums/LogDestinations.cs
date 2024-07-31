namespace Rabobank.Compliancy.Domain.Enums;

public enum LogDestinations
{
    AuditDeploymentLog,
    AuditLoggingErrorLog,
    AuditLoggingHookFailureLog,
    AuditPoisonMessagesLog,
    AuditPullRequestApproversLog,
    ComplianceScannerOnlineErrorLog,
    CompliancyCis,
    CompliancyItems,
    CompliancyPipelines,
    CompliancyPrinciples,
    CompliancyRules,
    DecoratorErrorLog,
    DeviationsLog,
    ErrorHandlingLog,
    PipelineBreakerComplianceLog,
    PipelineBreakerErrorLog,
    PipelineBreakerLog,
    Sm9ChangesErrorLog,
    ValidateGatesErrorLog
}