using Rabobank.Compliancy.Domain.Builders;
using System.Xml.Linq;

namespace Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

/// <inheritdoc/>
internal abstract class ExpectedTaskBase : IExpectedTask
{
    protected virtual DefinedTaskBuilder DefinedTaskBuilder { get; }

    protected abstract string TaskName { get; }

    protected abstract Guid TaskId { get; }

    protected readonly DefinedPipelineTask _expectedPipelineTask;

    protected ExpectedTaskBase()
    {
        DefinedTaskBuilder ??= new DefinedTaskBuilder(TaskId, TaskName);
        _expectedPipelineTask = DefinedTaskBuilder.Build();
    }

    public virtual bool IsSameTask(PipelineTask taskToEvaluate)
    {
        if (taskToEvaluate == null)
        {
            return false;
        }
        if (taskToEvaluate.Id != null)
        {
            return taskToEvaluate.Id.Equals(_expectedPipelineTask.Id);
        }
        if (taskToEvaluate.Name != null)
        {
            return taskToEvaluate.Name.Equals(_expectedPipelineTask.Name, StringComparison.OrdinalIgnoreCase) ||
                   taskToEvaluate.Name.Equals(_expectedPipelineTask.Id.ToString(), StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
}