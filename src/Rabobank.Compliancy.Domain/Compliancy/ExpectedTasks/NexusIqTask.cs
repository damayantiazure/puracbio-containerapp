using Rabobank.Compliancy.Domain.Builders;

namespace Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

internal class NexusIqTask : ExpectedTaskBase
{
    protected override string TaskName => "NexusIqPipelineTask";

    protected override Guid TaskId => new("4f40d1a2-83b0-4ddc-9a77-e7f279eb1802");
}