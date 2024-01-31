using Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

namespace Rabobank.Compliancy.Domain.Compliancy.Rules;

public class PipelineHasSm9CreateTask : PipelineHasTasksRule
{
    private static readonly IEnumerable<IExpectedTask> _expectedTasks = new List<IExpectedTask> { new Sm9CreateTask() };

    public PipelineHasSm9CreateTask() : base(_expectedTasks)
    {
    }
}