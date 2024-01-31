using Rabobank.Compliancy.Domain.Builders;

namespace Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

internal class DbbBuildTask : ExpectedTaskBase
{
    protected override Guid TaskId => new("f0ed76ac-b927-42fa-a758-a36c1838a13b");

    protected override string TaskName => "dbb-build";
}