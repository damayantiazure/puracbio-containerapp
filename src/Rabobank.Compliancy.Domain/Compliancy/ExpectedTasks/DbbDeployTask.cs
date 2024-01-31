using Rabobank.Compliancy.Domain.Builders;

namespace Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

internal class DbbDeployTask : ExpectedTaskWithInput
{
    protected override DefinedTaskBuilder DefinedTaskBuilder => new DefinedTaskBuilder(TaskId, TaskName)
        .WithInvariantValueInput("OrganizationName")
        .WithInvariantValueInput("ProjectId")
        .WithInvariantValueInput("PipelineId");

    protected override Guid TaskId => new("206089fc-dcf1-4d0a-bc10-135adf95db3c");

    protected override string TaskName => "dbb-deploy-prod";
}