namespace Rabobank.Compliancy.Infrastructure.Models.Yaml;

public class BuildModel
{
    public string Build { get; set; }
    public string Type { get; set; }
    public string Connection { get; set; }
    public string Source { get; set; }
    public string Version { get; set; }
    public string Branch { get; set; }
    public string Trigger { get; set; }
}