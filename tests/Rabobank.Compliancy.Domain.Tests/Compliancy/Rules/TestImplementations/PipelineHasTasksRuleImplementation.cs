using Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;
using Rabobank.Compliancy.Domain.Compliancy.Rules;

namespace Rabobank.Compliancy.Domain.Tests.Compliancy.Rules.TestImplementations;

public class PipelineHasTasksRuleImplementation : PipelineHasTasksRule
{
    public PipelineHasTasksRuleImplementation(IEnumerable<IExpectedTask> expectedTasks) : base(expectedTasks)
    {
    }
}