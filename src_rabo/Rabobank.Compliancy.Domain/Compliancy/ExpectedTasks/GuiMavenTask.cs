using Rabobank.Compliancy.Domain.Builders;

namespace Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

internal class GuiMavenTask : ExpectedTaskWithInput
{
    protected override DefinedTaskBuilder DefinedTaskBuilder => new DefinedTaskBuilder(TaskId, TaskName)
        .WithSpecificValueInput("sqAnalysisEnabled", "true");

    protected override Guid TaskId => new("ac4ee482-65da-4485-a532-7b085873e532");

    protected override string TaskName => "GuiMavenTask";
}