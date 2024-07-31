using Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

namespace Rabobank.Compliancy.Domain.Compliancy.Rules;

public class BuildPipelineHasNexusIqTask : PipelineHasTasksRule
{
    private static readonly IEnumerable<IExpectedTask> _expectedTasks = new List<IExpectedTask>
    {
        new NexusIqTask()
    };

    public BuildPipelineHasNexusIqTask() : base(_expectedTasks)
    {
    }
}