using Rabobank.Compliancy.Domain.Builders;

namespace Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

internal class Sm9ApproveTask : ExpectedTaskBase
{
    protected override string TaskName => "SM9 - Approve";

    protected override Guid TaskId => new("73cb0c6a-0623-4814-8774-57dc1ef33858");
}