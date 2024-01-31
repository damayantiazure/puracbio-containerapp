namespace Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

/// <summary>
/// Represents a <see cref="PipelineTask"/> that has all properties required for comparison defined and not <see langword="null"/>
/// </summary>
internal class DefinedPipelineTask
{
    private readonly Dictionary<string, ExpectedInputValue> _inputs = new();

    internal Guid Id { get; set; }

    internal string Name { get; set; }

    internal Dictionary<string, ExpectedInputValue> Inputs { get { return _inputs; } }

    internal DefinedPipelineTask(Guid taskId, string name)
    {
        Id = taskId;
        Name = ValidateName(name);
    }

    internal void AddInput(string inputKey, ExpectedInputValue inputValue)
    {
        _inputs.Add(inputKey, inputValue);
    }

    private static string ValidateName(string name)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name of a Task cannot be Null, Empty or Whitespace", nameof(name));
        }
        return name;
    }
}