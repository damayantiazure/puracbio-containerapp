using Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

namespace Rabobank.Compliancy.Domain.Compliancy.Rules;

public class ClassicReleasePipelineFollowsMainframeCobolReleaseProcess : PipelineHasTasksRule
{
    private static IEnumerable<IExpectedTask> _expectedTasks =
        new List<IExpectedTask> { new DbbDeployTask() };

    public ClassicReleasePipelineFollowsMainframeCobolReleaseProcess() : base(_expectedTasks)
    {
    }
}