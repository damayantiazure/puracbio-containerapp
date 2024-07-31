using Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

namespace Rabobank.Compliancy.Domain.Compliancy.Rules;

public class BuildPipelineFollowsMainframeCobolProcess : PipelineHasTasksRule
{
    private static IEnumerable<IExpectedTask> _expectedTasks =
        new List<IExpectedTask> { new DbbBuildTask(), new DbbPackageTask() };

    public BuildPipelineFollowsMainframeCobolProcess() : base(_expectedTasks)
    {
    }
}