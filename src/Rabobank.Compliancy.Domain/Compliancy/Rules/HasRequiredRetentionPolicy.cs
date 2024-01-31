using Rabobank.Compliancy.Domain.Compliancy.Evaluatables;

namespace Rabobank.Compliancy.Domain.Compliancy.Rules;

public abstract class HasRequiredRetentionPolicy : IRule<SettingsEvaluatable>
{
    private const int ExpectedMinimalRetention = 450;

    public EvaluationResult Evaluate(SettingsEvaluatable evaluatable)
    {
        var retentionSettings = evaluatable.GetSettings<RetentionSettings>();
        if (retentionSettings == null)
        {
            return new EvaluationResult { Passed = false };
        }

        return new EvaluationResult { Passed = retentionSettings.DaysToKeepRuns >= ExpectedMinimalRetention };
    }
}