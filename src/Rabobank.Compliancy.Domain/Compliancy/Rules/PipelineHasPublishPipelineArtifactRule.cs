using Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

namespace Rabobank.Compliancy.Domain.Compliancy.Rules;

public class PipelineHasPublishPipelineArtifactRule : PipelineHasTasksRule
{
    private static readonly IEnumerable<IExpectedTask> _expectedTasks = new List<IExpectedTask> { new PublishPipelineArtifactTask() };

    public PipelineHasPublishPipelineArtifactRule() : base(_expectedTasks)
    {
    }
}