using Rabobank.Compliancy.Domain.Builders;

namespace Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

internal class PostAnalysisTask : ExpectedTaskWithInput
{
    protected override DefinedTaskBuilder DefinedTaskBuilder => new DefinedTaskBuilder(TaskId, TaskName)
        .WithSpecificValueInput("CredScan", "true");

    protected override string TaskName => "PostAnalysis";

    protected override Guid TaskId => new("dbe519ee-a2e4-43f5-8e1a-949bd935b736");
}