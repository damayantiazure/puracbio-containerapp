using Rabobank.Compliancy.Domain.Builders;

namespace Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

internal class SonarQubeAnalyzeTask : ExpectedTaskBase
{
    protected override string TaskName => "SonarQubeAnalyze";

    protected override Guid TaskId => new("15b84ca1-b62f-4a2a-a403-89b77a063157");
}