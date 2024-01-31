using Rabobank.Compliancy.Domain.Builders;

namespace Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

internal abstract class ExpectedTaskWithInput : ExpectedTaskBase
{
    public override bool IsSameTask(PipelineTask taskToEvaluate)
    {
        return base.IsSameTask(taskToEvaluate) && HasExpectedInputs(taskToEvaluate);
    }

    private bool HasExpectedInputs(PipelineTask taskToEvaluate)
    {
        var inputs = _expectedPipelineTask.Inputs;
        foreach (var expectedInput in inputs)
        {
            if (!taskToEvaluate.Inputs.Any(input => input.Key.Equals(expectedInput.Key, StringComparison.OrdinalIgnoreCase) &&
                                                    HasCorrectInput(input.Value, expectedInput.Value)))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// This method checks if a string value provided 'valueToEvaluate' is the same as a the provided 'expectedInput'.
    /// There are two types of comparision
    /// - compare the exact value
    /// - check if 'valueToEvaluate' is not null or empty
    /// Both checks ignore the casing.
    /// </summary>        
    private static bool HasCorrectInput(string valueToEvaluate, ExpectedInputValue expectedInput)
    {
        return (expectedInput.ValidationType == InputValueValidationType.HasExactValue &&
                expectedInput.Value.Equals(valueToEvaluate, StringComparison.OrdinalIgnoreCase)) ||
               (expectedInput.ValidationType == InputValueValidationType.IsNotNullOrEmpty &&
                !string.IsNullOrEmpty(valueToEvaluate));
    }
}