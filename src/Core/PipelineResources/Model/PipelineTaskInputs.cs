using System.Collections.Generic;
using System.Linq;

namespace Rabobank.Compliancy.Core.PipelineResources.Model;

public class PipelineTaskInputs
{
    public string FullTaskName { get; set; }
    public Dictionary<string, string> Inputs { get; set; }
    public bool Enabled { get; set; }

    /// <summary>
    /// The FullTaskName has the following format [optionalnames][.][taskname/taskId]@[version]
    /// This method checks if a fullTaskName matches a given taskname taking into account the format and if it is enabled
    /// </summary>        
    public bool HasTaskNameOrIdAndIsEnabled(string taskNameOrId)
    {
        if (FullTaskName == null)
        {
            return false;
        }

        var taskNameWithPrefix = FullTaskName.Split('@')[0];
        var taskName = taskNameWithPrefix.Split('.').Last();
        return taskName.Equals(taskNameOrId, System.StringComparison.OrdinalIgnoreCase) && Enabled;
    }
}