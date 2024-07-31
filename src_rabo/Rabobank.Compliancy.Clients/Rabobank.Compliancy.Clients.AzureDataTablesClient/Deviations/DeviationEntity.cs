namespace Rabobank.Compliancy.Clients.AzureDataTablesClient.Deviations;

public class DeviationEntity : BaseEntity
{
    /// <summary>
    ///     WARNING!
    ///     This constructor is used when reading the entity from the table storage. When creating a new entity to insert
    ///     please use the parameterized constructor.
    /// </summary>
    public DeviationEntity()
    {
    }

    /// <summary>
    ///     Creates a new DeviationEntity. The rowKey is calculated based on the organization,
    ///     projectId, ruleName, itemId, ciIdentifier and foreignProjectId parameters.
    /// </summary>
    public DeviationEntity(string organization, string projectName, string? ruleName,
        string? itemId, string? ciIdentifier, Guid projectId, string? comment, string? reason,
        string? updatedBy, Guid? foreignProjectId)
        : base(projectId, organization, projectId, ruleName, itemId, ciIdentifier, foreignProjectId)
    {
        Organization = organization;
        ProjectName = projectName;
        RuleName = ruleName;
        ItemId = itemId;
        CiIdentifier = ciIdentifier;
        ProjectId = projectId.ToString();
        Comment = comment;
        Reason = reason;
        UpdatedBy = updatedBy;
        ForeignProjectId = foreignProjectId?.ToString();
    }

    public string? Organization { get; set; }
    public string? ProjectName { get; set; }
    public string? ProjectId { get; set; }
    public string? RuleName { get; set; }
    public string? ItemId { get; set; }
    public string? Comment { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? Date { get; set; }
    public string? Reason { get; set; }
    public string? ReasonNotApplicable { get; set; }
    public string? ReasonNotApplicableOther { get; set; }
    public string? ReasonOther { get; set; }
    public string? CiIdentifier { get; set; }
    public string? ForeignProjectId { get; set; }
}