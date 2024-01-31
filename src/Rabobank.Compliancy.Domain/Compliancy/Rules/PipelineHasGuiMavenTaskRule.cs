using Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

namespace Rabobank.Compliancy.Domain.Compliancy.Rules;

public class PipelineHasGuiMavenTaskRule : PipelineHasTasksRule
{
    private static readonly IEnumerable<IExpectedTask> _expectedTasks = new List<IExpectedTask> { new GuiMavenTask() };

    public PipelineHasGuiMavenTaskRule() : base(_expectedTasks)
    {
    }
}