using Rabobank.Compliancy.Domain.Compliancy.Evaluatables;
using Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;
using Rabobank.Compliancy.Domain.Extensions;

namespace Rabobank.Compliancy.Domain.Compliancy.Rules;

public abstract class PipelineHasTasksRule : IRule<TaskContainingEvaluatable>
{
    private readonly IEnumerable<IExpectedTask> _expectedTasks;

    protected PipelineHasTasksRule(IEnumerable<IExpectedTask> expectedTasks)
    {
        _expectedTasks = expectedTasks.EnsureNonEmptyCollection();
    }

    public EvaluationResult Evaluate(TaskContainingEvaluatable evaluatable)
    {
        var tasksToEvaluate = evaluatable.GetPipelineTasks().ToList();
        foreach (var expectedTask in _expectedTasks)
        {
            if (!tasksToEvaluate.Any(expectedTask.IsSameTask))
            {
                return new EvaluationResult { Passed = false };
            }
        }

        return new EvaluationResult { Passed = true };
    }
}