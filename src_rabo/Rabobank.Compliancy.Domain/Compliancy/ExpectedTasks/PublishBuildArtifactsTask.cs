using Rabobank.Compliancy.Domain.Builders;

namespace Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

internal class PublishBuildArtifactsTask : ExpectedTaskBase
{
    protected override string TaskName => "PublishBuildArtifacts";

    protected override Guid TaskId => new("2ff763a7-ce83-4e1f-bc89-0ae63477cebe");
}