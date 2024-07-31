#nullable enable

using Rabobank.Compliancy.Domain.Enums;

namespace Rabobank.Compliancy.Domain.Compliancy;

public class Pipeline : PipelineResource, IProtectedResource
{
    /// <summary>
    ///     Unique Identifier of a Pipeline in a given Project Scope
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     A list of all (retained) Runs that have been initiated for this Pipeline
    /// </summary>
    public List<Run> Runs { get; set; } = new();

    /// <summary>
    ///     The Unique Technical variety of a Pipeline as defined by a resource where the pipeline exists
    /// </summary>
    public PipelineProcessType DefinitionType { get; set; }

    /// <summary>
    ///     The Body of a Pipeline Run if a new Run were started for this Pipeline with all the current default settings
    /// </summary>
    public PipelineBody? DefaultRunContent { get; set; }

    /// <summary>
    ///     Represents the <see cref="GitRepo" /> that hosts the source code of this object. Can be null.
    /// </summary>
    public GitRepo? SourceGitRepo { get; set; }

    /// <summary>
    ///     Contains all resources consumed by this pipeline, including other pipelines, GitRepos and specific runs
    /// </summary>
    public IEnumerable<PipelineResource>? ConsumedResources { get; set; }

    /// <summary>
    ///     Contains all settings for a pipeline, such as retention settings
    /// </summary>
    public IEnumerable<ISettings>? Settings { get; set; }

    /// <summary>
    ///     Getter for the path of the build or release definition.
    /// </summary>
    public string? Path { get; init; }
}