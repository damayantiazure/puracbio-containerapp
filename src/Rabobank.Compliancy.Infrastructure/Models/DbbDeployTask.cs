using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Infrastructure.Models;

internal class DbbDeployTask : PipelineReferenceTask
{
    private const string Name = "dbb-deploy-prod";
    private static readonly Guid TaskId = new("206089fc-dcf1-4d0a-bc10-135adf95db3c");

    public DbbDeployTask(PipelineTask task) : base(task, new[] { "projectid" }, new[] { "pipelineid" })
    {
        TaskName = Name;
        Id = TaskId;
    }

    public static bool IsDbbDeployTask(PipelineTask task)
    {
        return task.Name.Contains(Name, StringComparison.InvariantCultureIgnoreCase) || task.Id == TaskId;
    }
}