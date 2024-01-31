#nullable enable

namespace Rabobank.Compliancy.Domain.Compliancy.Exclusions;

/// <summary>
/// The exclusion model to be used to create a new exclusion entity in the table storage.
/// </summary>
public class Exclusion
{
    private const int _twentyFourHours = -24;

    /// <summary>
    /// Initializes a new instance of the <see cref="Exclusion"/> class.
    /// </summary>
    public Exclusion(string organization, Guid projectId, string pipelineId, string pipelineType)
    {
        Organization = organization;
        ProjectId = projectId;
        PipelineId = pipelineId;
        PipelineType = pipelineType;
    }

    /// <summary>
    /// Getter and setter for the name of the organization.
    /// </summary>
    public string Organization { get; init; }

    /// <summary>
    /// Getter and setter of the project identifier.
    /// </summary>
    public Guid ProjectId { get; init; }

    /// <summary>
    /// Getter and setter of the pipeline identifier.
    /// </summary>
    public string PipelineId { get; init; }

    /// <summary>
    /// Getter and setter of the pipeline type.
    /// </summary>
    public string PipelineType { get; init; }

    /// <summary>
    /// Getter and setter for the exclusion reason of the requester.
    /// </summary>
    public string? ExclusionReasonRequester { get; set; }

    /// <summary>
    /// Getter and setter for the email address of the exclusion requester.
    /// </summary>
    public string? Requester { get; set; }

    /// <summary>
    /// Getter and setter for the exclusion reason of the approver.
    /// </summary>
    public string? ExclusionReasonApprover { get; set; }

    /// <summary>
    /// Getter and setter for the email address of the exclusion approver.
    /// </summary>
    public string? Approver { get; set; }

    /// <summary>
    /// Getter and setter for the pipeline run identifier.
    /// </summary>
    public string? RunId { get; set; }

    /// <summary>
    /// Getter and setter for the timestamp property.
    /// </summary>
    public DateTimeOffset? Timestamp { get; init; }

    /// <summary>
    /// Getter to validated if the eclusion has been approved.
    /// </summary>
    public bool IsApproved =>
        RunId != null && Requester != null && Approver != null && Requester.ToLowerInvariant() != Approver.ToLowerInvariant();

    /// <summary>
    /// Getter to validate if the exclusion has been expired.
    /// </summary>
    public bool IsExpired =>
       Timestamp != null && Timestamp?.CompareTo(DateTime.Now.AddHours(_twentyFourHours)) < 0;
}