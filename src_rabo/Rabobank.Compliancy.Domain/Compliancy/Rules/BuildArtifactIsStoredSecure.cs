using Rabobank.Compliancy.Domain.Compliancy.Evaluatables;

namespace Rabobank.Compliancy.Domain.Compliancy.Rules;

public class BuildArtifactIsStoredSecure : IRule<TaskContainingEvaluatable>
{
    private readonly IEnumerable<PipelineHasTasksRule> _rules = new List<PipelineHasTasksRule>()
    {
        new PipelineHasPublishBuildArtifactsRule(),
        new PipelineHasPublishPipelineArtifactRule()
    };

    public EvaluationResult Evaluate(TaskContainingEvaluatable evaluatable)
    {
        var result = _rules.Any(rule => rule.Evaluate(evaluatable).Passed);
        return new EvaluationResult { Passed = result };
    }
}