using Rabobank.Compliancy.Domain.Builders;
using Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

namespace Rabobank.Compliancy.Domain.Tests.Compliancy.ExpectedTasks.Implementations;

internal class ExpectedTaskImplementation : ExpectedTaskBase
{
    public string GettableTaskName = "Test Task";

    public Guid GettableTaskId = Guid.NewGuid();

    protected override string TaskName => GettableTaskName;

    protected override Guid TaskId => GettableTaskId;
}