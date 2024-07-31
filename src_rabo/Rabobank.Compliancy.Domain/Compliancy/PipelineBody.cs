#nullable enable

namespace Rabobank.Compliancy.Domain.Compliancy;

/// <summary>
/// This class contains the content or body of a "pipeline" when it runs, has run or would run.
/// In case a Run has not taken place, it would contain the information of "What if a certain pipeline would be kicked off now?" or 
/// "What if a run would be initiated for this pipeline now?"
/// We expect the content to be built roughly around the concept:
/// 1 pipeline run has 1 to n stages
/// 1 stage has 1 to n jobs
/// 1 job has 1 to n tasks
/// a pipeline run has 0 to n resources, like GitRepos and the (output of) other pipeline runs
/// </summary>
public class PipelineBody
{
    public IEnumerable<Stage>? Stages { get; set; }
    public IEnumerable<PipelineResource>? Resources { get; set; }
    public IEnumerable<PipelineTask>? Tasks { get; set; }
    public IEnumerable<Gate>? Gates { get; set; }
    public IEnumerable<ITrigger>? Triggers { get; set; }

    /// <summary>
    /// Indicates if the pipeline uses an artifact other that a build artifact
    /// </summary>
    public bool UsesNonBuildArtifact { get; set; }
}