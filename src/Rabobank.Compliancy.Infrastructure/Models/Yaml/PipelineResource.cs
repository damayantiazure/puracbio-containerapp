namespace Rabobank.Compliancy.Infrastructure.Models.Yaml;

public class PipelineResource
{
    /// <summary>
    /// Identifier for the resource used in pipeline resource variables
    /// </summary>
    public string Pipeline { get; set; }

    /// <summary>
    /// The name of the pipeline that produces an artifact
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// The name of the project for the source; optional for current project
    /// </summary>
    public string Project { get; set; }
}