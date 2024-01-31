using Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

namespace Rabobank.Compliancy.Domain.Compliancy.Rules;

public class PipelineHasSm9ApproveTask : PipelineHasTasksRule
{
    private static readonly IEnumerable<IExpectedTask> _expectedTasks = new List<IExpectedTask> { new Sm9ApproveTask() };

    public PipelineHasSm9ApproveTask() : base(_expectedTasks)
    {
    }
}