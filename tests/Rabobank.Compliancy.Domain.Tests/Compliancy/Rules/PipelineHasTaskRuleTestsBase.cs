using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Domain.Tests.Compliancy.Rules;

public class PipelineHasTaskRuleTestsBase
{
    protected static PipelineTask CreateTask(string taskName, Dictionary<string, string> inputs = null)
    {
        inputs ??= new Dictionary<string, string>();
        return new PipelineTask
        {
            Name = taskName,
            Inputs = inputs
        };
    }

    protected static PipelineTask CreateTask(Guid taskId, Dictionary<string, string> inputs = null)
    {
        inputs ??= new Dictionary<string, string>();
        return new PipelineTask
        {
            Id = taskId,
            Inputs = inputs
        };
    }
}