namespace Rabobank.Compliancy.Infrastructure.Models.Yaml;

public class InputModel
{
    public string BuildType { get; set; }

    public Guid Project { get; set; }

    public long Pipeline { get; set; }

    public string RunVersion { get; set; }

    public string RunBranch { get; set; }

    public string Path { get; set; }
}