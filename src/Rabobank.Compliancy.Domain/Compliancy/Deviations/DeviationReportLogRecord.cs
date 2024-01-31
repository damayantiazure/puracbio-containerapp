#nullable enable

namespace Rabobank.Compliancy.Domain.Compliancy.Deviations;

public class DeviationReportLogRecord
{
    public DeviationReportLogRecord(DeviationReportLogRecordType recordType, Guid projectId, string ruleName, string itemId, string ciIdentifier,
        string comment, DeviationReason reason)
    {
        RecordType = recordType;
        ProjectId = projectId;
        RuleName = ruleName;
        ItemId = itemId;
        CiIdentifier = ciIdentifier;
        Comment = comment;
        Reason = reason;
    }

    public DeviationReportLogRecordType RecordType { get; }
    public Guid ProjectId { get; }
    public string RuleName { get; }
    public string ItemId { get; }
    public Guid? ItemProjectId { get; set; }
    public string CiIdentifier { get; }
    public string Comment { get; }
    public DeviationReason Reason { get; }
    public DeviationApplicabilityReason? ReasonNotApplicable { get; set; }
    public string? ReasonNotApplicableOther { get; set; }
    public string? ReasonOther { get; set; }
}