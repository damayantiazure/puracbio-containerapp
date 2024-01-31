using Rabobank.Compliancy.Domain.Builders;

namespace Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

internal class PublishPipelineArtifactTask : ExpectedTaskBase
{
    protected override string TaskName => "PublishPipelineArtifact";

    protected override Guid TaskId => new("ecdc45f6-832d-4ad9-b52b-ee49e94659be");
}