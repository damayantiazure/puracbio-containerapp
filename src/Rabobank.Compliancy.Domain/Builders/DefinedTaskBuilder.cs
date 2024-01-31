using Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

namespace Rabobank.Compliancy.Domain.Builders;

internal class DefinedTaskBuilder
{
    private const string TaskIdAndTaskNameCannotBothBeNullError = "Task ID and task name cannot be null or empty.";
    private readonly DefinedPipelineTask _task;

    internal DefinedTaskBuilder(Guid taskId, string taskName)
    {
        if (taskId == Guid.Empty || string.IsNullOrEmpty(taskName))
        {
            throw new ArgumentException(TaskIdAndTaskNameCannotBothBeNullError);
        }

        _task = new DefinedPipelineTask(taskId, taskName);
    }

    internal DefinedTaskBuilder WithInvariantValueInput(string inputKey)
    {
        _task.AddInput(inputKey, new ExpectedInputValue());
        return this;
    }

    internal DefinedTaskBuilder WithSpecificValueInput(string inputKey, ExpectedInputValue inputValue)
    {
        _task.AddInput(inputKey, inputValue);
        return this;
    }

    internal DefinedTaskBuilder WithSpecificValueInput(string inputKey, string expectedvalue)
    {
        _task.AddInput(inputKey, new ExpectedInputValue(expectedvalue));
        return this;
    }

    internal DefinedPipelineTask Build() => _task;
}