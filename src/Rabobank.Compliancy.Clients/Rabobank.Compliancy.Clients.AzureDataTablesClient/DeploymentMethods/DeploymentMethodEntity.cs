namespace Rabobank.Compliancy.Clients.AzureDataTablesClient.DeploymentMethods;

public class DeploymentMethodEntity : BaseEntity
{
    public static readonly string Prod = "PROD";
    public static readonly string NonProd = "NON-PROD";

    /// <summary>
    ///     WARNING!
    ///     This constructor is used when reading the entity from the table storage. When creating a new entity to insert
    ///     please use the parameterized constructor.
    /// </summary>
    public DeploymentMethodEntity()
    {
    }

    /// <summary>
    ///     NON-PROD
    ///     Creates a new PipelineRegistrationEntity specifically for non-prod pipelines.
    /// </summary>
    public DeploymentMethodEntity(string organization, string? ciIdentifier, Guid projectId, int pipelineId,
        string pipelineType, string stageId)
        : base(string.IsNullOrEmpty(ciIdentifier) ? NonProd : Prod,
            ciIdentifier, projectId, pipelineId, pipelineType, stageId)
    {
        Organization = organization;
        CiIdentifier = ciIdentifier;
        ProjectId = projectId.ToString();
        PipelineId = pipelineId.ToString();
        PipelineType = pipelineType;
        StageId = stageId;
    }

    public string? Organization { get; set; }
    public string? ProjectId { get; set; }
    public string? PipelineId { get; set; }
    public string? PipelineType { get; set; }
    public string? StageId { get; set; }
    public bool? ToBeScanned { get; set; }
    public string? CiIdentifier { get; set; }
    public string? CiName { get; set; }
    public string? CiSubtype { get; set; }
    public bool? IsSoxApplication { get; set; }
    public string? AssignmentGroup { get; set; }
    public string? AicRating { get; set; }
    public string? RuleProfileName { get; set; }
}