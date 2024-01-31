using Rabobank.Compliancy.Domain.Builders;

namespace Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

internal class Sm9CreateTask : ExpectedTaskBase
{
    protected override string TaskName => "SM9 - Create";

    protected override Guid TaskId => new("d0c045b6-d01d-4d69-882a-c21b18a35472");
}