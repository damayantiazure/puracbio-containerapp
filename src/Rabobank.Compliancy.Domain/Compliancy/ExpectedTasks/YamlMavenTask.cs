using Rabobank.Compliancy.Domain.Builders;

namespace Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

internal class YamlMavenTask : ExpectedTaskWithInput
{
    protected override DefinedTaskBuilder DefinedTaskBuilder => new DefinedTaskBuilder(TaskId, TaskName)
        .WithSpecificValueInput("sonarQubeRunAnalysis", "true");

    protected override string TaskName => "Maven";

    protected override Guid TaskId => new("15b84ca1-b62f-4a2a-a403-89b77a063157");
}