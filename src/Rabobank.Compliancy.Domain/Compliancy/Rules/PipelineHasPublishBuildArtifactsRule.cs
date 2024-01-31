using Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

namespace Rabobank.Compliancy.Domain.Compliancy.Rules;

public class PipelineHasPublishBuildArtifactsRule : PipelineHasTasksRule
{
    private static readonly IEnumerable<IExpectedTask> _expectedTasks = new List<IExpectedTask> { new PublishBuildArtifactsTask() };

    public PipelineHasPublishBuildArtifactsRule() : base(_expectedTasks)
    {
    }
}