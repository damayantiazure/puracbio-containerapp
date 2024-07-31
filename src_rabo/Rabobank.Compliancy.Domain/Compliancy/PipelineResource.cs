namespace Rabobank.Compliancy.Domain.Compliancy;

/// <summary>
/// A PipelineResource is an object that represents any object that can be consumed by a pipeline during a run.
/// </summary>
public abstract class PipelineResource
{
    /// <summary>
    /// This property is the displayname that this resource is known under
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// This property is a reference to the Project that this resource exists in
    /// </summary>
    public Project Project { get; set; }
    /// <summary>
    /// This property is a reference to any pipelines that consume this resource
    /// </summary>
    public Pipeline[] ConsumingPipelines { get; set; }
}