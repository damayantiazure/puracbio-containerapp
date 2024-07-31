namespace Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

/// <summary>
/// Defines a PipelineTask that is Expected as part of a Rule
/// Offers a method to compare itself to a PipelineTask
/// </summary>
public interface IExpectedTask
{
    bool IsSameTask(PipelineTask taskToEvaluate);
}