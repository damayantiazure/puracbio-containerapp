using System.Collections.Generic;

namespace Rabobank.Compliancy.Core.PipelineResources.Model;

public interface IPipelineHasTaskRule
{
    string TaskId { get; }
    string TaskName { get; }
    Dictionary<string, string> Inputs { get; }

    /// <summary>
    ///
    /// </summary>
    bool IgnoreInputValues { get; }
}