using Rabobank.Compliancy.Domain.Builders;

namespace Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

internal class DbbPackageTask : ExpectedTaskBase
{
    protected override Guid TaskId => new("dc5c403b-4cd3-48f2-9dcc-4405e1b6f981");

    protected override string TaskName => "dbb-package";
}