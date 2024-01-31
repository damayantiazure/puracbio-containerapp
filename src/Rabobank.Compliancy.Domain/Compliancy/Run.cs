namespace Rabobank.Compliancy.Domain.Compliancy;

/// <summary>
/// A Run is an instance of a Pipeline
/// </summary>
public class Run : PipelineResource
{
    /// <summary>
    /// Unique Identifier of a Run in a given Project Scope
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Pipeline of which the Run object is an Instance
    /// </summary>
    public Pipeline Pipeline { get; set; }

    /// <summary>
    /// The pipeline contents of the pipeline when it runs, has run or would run
    /// </summary>
    public PipelineBody RunBody { get; set; }
}