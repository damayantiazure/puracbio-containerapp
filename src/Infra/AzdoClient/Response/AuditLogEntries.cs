namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class AuditLogEntries
{
    public AuditLogEntry[] DecoratedAuditLogEntries { get; set; }
    public string ContinuationToken { get; set; }
}