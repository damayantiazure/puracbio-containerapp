using System.Collections.Generic;

namespace Rabobank.Compliancy.Core.PipelineResources.Model;

public class PipelineHasTaskRule : IPipelineHasTaskRule
{
    public string TaskId { get; init; }
    public string TaskName { get; init; }
    public Dictionary<string, string> Inputs { get; init; }

    /// <inheritdoc/>
    public bool IgnoreInputValues { get; init; }

    public PipelineHasTaskRule(string taskId)
    {
        TaskId = taskId;
    }
}