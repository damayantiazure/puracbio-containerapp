namespace Rabobank.Compliancy.Clients.AzureDataTablesClient.Exclusions;

public class ExclusionEntity : BaseEntity
{
    // definition of the partionKey
    private const string Exclusion = nameof(Exclusion);

    public ExclusionEntity() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExclusionEntity"/> class. The rowkey is calculated based on the organization,
    /// projectId, pileineId and pipelineType parameters.
    /// </summary>
    public ExclusionEntity(string organization, Guid projectId, string pipelineId, string pipelineType)
        : base(Exclusion, organization, projectId, pipelineId, pipelineType)
    {
        Organization = organization;
        ProjectId = projectId;
        PipelineId = pipelineId;
        PipelineType = pipelineType;
    }

    /// <summary>
    /// The getter and setter for the name of the organization.
    /// </summary>
    public string Organization { get; set; } = default!;

    /// <summary>
    /// The getter and setter for the project identifier.
    /// </summary>
    public Guid ProjectId { get; set; } = Guid.Empty!;

    /// <summary>
    /// The getter and setter for the pipeline identifier.
    /// </summary>
    public string PipelineId { get; set; } = default!;

    /// <summary>
    /// The getter and setter for the pipeline type.
    /// </summary>
    public string PipelineType { get; set; } = default!;

    /// <summary>
    ///  The getter and setter for the reason of the requester.
    /// </summary>
    public string? ExclusionReasonRequester { get; set; }

    /// <summary>
    /// The getter and setter for the requester mailaddress of the exclusion.
    /// </summary>
    public string? Requester { get; set; }

    /// <summary>
    /// The getter and setter for the reason of the approver.
    /// </summary>
    public string? ExclusionReasonApprover { get; set; }

    /// <summary>
    /// The getter and setter for the approvers mailaddress of the exclusion.
    /// </summary>
    public string? Approver { get; set; }

    /// <summary>
    /// The getter and setter for the identifier of the pipeline run.
    /// </summary>
    public string? RunId { get; set; }
}