using Rabobank.Compliancy.Domain.Builders;

namespace Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

internal class FortifyTask : ExpectedTaskBase
{
    protected override Guid TaskId => new("818386e5-c8a5-46c3-822d-954b3c8fb130");

    protected override string TaskName => "FortifySCA";
}