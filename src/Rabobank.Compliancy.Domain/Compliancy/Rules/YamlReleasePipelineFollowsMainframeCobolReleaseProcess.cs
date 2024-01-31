using Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

namespace Rabobank.Compliancy.Domain.Compliancy.Rules;

internal class YamlReleasePipelineFollowsMainframeCobolReleaseProcess : PipelineHasTasksRule
{
    private static IEnumerable<IExpectedTask> _expectedTasks =
        new List<IExpectedTask> { new DbbDeployTask() };

    public YamlReleasePipelineFollowsMainframeCobolReleaseProcess() : base(_expectedTasks)
    {
    }
}