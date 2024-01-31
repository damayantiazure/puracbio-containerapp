using Rabobank.Compliancy.Domain.Builders;

namespace Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

internal class CredScanTask : ExpectedTaskBase
{
    protected override Guid TaskId => new("f0462eae-4df1-45e9-a754-8184da95ed01");

    protected override string TaskName => "CredScan";
}