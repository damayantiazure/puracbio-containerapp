using Rabobank.Compliancy.Domain.Compliancy.Evaluatables;
namespace Rabobank.Compliancy.Domain.Compliancy.Rules;

internal class ClassicReleasePipelineUsesBuildArtifact : IRule<ResourceEvaluatable>
{
    public EvaluationResult Evaluate(ResourceEvaluatable evaluatable)
    {
        bool isClassicAndUsesNonBuildArtifact = evaluatable.PipelineUsesNonBuildArtifact &&
                                                evaluatable.PipelineProcessType == Enums.PipelineProcessType.DesignerRelease;

        return new EvaluationResult { Passed = !isClassicAndUsesNonBuildArtifact };
    }
}