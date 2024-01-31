namespace Rabobank.Compliancy.Infrastructure.Models.Yaml;

public class StepModel
{
    public string Task { get; set; }

    public string DisplayName { get; set; }

    public bool? ContinueOnError { get; set; }

    public Dictionary<string, string> Inputs { get; set; }

    public string Download { get; set; }

    public string Script { get; set; }
}