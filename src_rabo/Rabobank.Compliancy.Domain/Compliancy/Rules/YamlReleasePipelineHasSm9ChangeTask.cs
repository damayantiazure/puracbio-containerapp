using Rabobank.Compliancy.Domain.Compliancy.Evaluatables;
using Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

namespace Rabobank.Compliancy.Domain.Compliancy.Rules;

public class YamlReleasePipelineHasSm9ChangeTask : IRule<TaskContainingEvaluatable>
{
    private readonly IEnumerable<PipelineHasTasksRule> _rules = new List<PipelineHasTasksRule>()
    {
        new PipelineHasSm9ApproveTask(),
        new PipelineHasSm9CreateTask()
    };

    public EvaluationResult Evaluate(TaskContainingEvaluatable evaluatable)
    {
        var result = _rules.Any(rule => rule.Evaluate(evaluatable).Passed);

        return new EvaluationResult { Passed = result };

    }
}