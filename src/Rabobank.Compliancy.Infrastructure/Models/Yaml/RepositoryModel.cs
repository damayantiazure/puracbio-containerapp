namespace Rabobank.Compliancy.Infrastructure.Models.Yaml;

public class RepositoryModel
{
    public string Repository { get; set; }
    public string Endpoint { get; set; }
    public string Trigger { get; set; }
    public string Ref { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
}