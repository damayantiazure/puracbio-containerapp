using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Infrastructure.Models;

internal class DownloadPipelineArtifactTask : PipelineReferenceTask
{
    private const string Name = "DownloadPipelineArtifact";
    private static readonly Guid TaskId = new("61f2a582-95ae-4948-b34d-a1b3c4f6a737");

    public DownloadPipelineArtifactTask(PipelineTask task) : base(task, new[] { "project" }, new[] { "pipeline", "definition" })
    {
        TaskName = Name;
        Id = TaskId;
    }

    public static bool IsDownloadPipelineArtifactTask(PipelineTask task)
    {
        return task.Name.Contains(Name, StringComparison.InvariantCultureIgnoreCase) || task.Id == TaskId;
    }
}