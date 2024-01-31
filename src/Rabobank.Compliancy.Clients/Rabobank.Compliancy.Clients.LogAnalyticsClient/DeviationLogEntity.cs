namespace Rabobank.Compliancy.Clients.LogAnalyticsClient;

public class DeviationLogEntity
{
    public string? RecordType { get; set; }
    public Guid? ProjectId { get; set; }
    public string? RuleName { get; set; }
    public string? ItemId { get; set; }
    public Guid? ItemProjectId { get; set; }
    public string? CiIdentifier { get; set; }
    public string? Comment { get; set; }
    public string? Reason { get; set; }
    public string? ReasonNotApplicable { get; set; }
    public string? ReasonNotApplicableOther { get; set; }
    public string? ReasonOther { get; set; }
}