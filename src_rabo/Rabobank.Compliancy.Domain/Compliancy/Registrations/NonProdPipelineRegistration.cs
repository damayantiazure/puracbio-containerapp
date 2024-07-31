namespace Rabobank.Compliancy.Domain.Compliancy.Registrations;

/// <summary>
/// Object represents a non prod registration of a pipeline.    
/// </summary>
public class NonProdPipelineRegistration : PipelineRegistration
{
    /// <summary>
    /// Indicates if this pipeline should be scanned for compliancy.
    /// </summary>
    public bool ShouldBeScanned { get; set; }

    /// <summary>
    /// The id of the stage which is registered
    /// </summary>
    public string StageId { get; set; }
}