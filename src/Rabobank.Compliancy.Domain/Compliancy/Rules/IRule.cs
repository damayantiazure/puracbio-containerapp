using Rabobank.Compliancy.Domain.Compliancy.Evaluatables;

namespace Rabobank.Compliancy.Domain.Compliancy.Rules;

internal interface IRule<in TEvaluatable> where TEvaluatable : IEvaluatable
{
    EvaluationResult Evaluate(TEvaluatable evaluatable);
}