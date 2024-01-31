namespace Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

internal class ExpectedInputValue
{
    internal InputValueValidationType ValidationType { get; }
    internal string Value { get; }

    internal ExpectedInputValue(string value)
    {
        ValidationType = InputValueValidationType.HasExactValue;
        Value = value;
    }

    internal ExpectedInputValue()
    {
        ValidationType = InputValueValidationType.IsNotNullOrEmpty;
        Value = string.Empty;
    }
}