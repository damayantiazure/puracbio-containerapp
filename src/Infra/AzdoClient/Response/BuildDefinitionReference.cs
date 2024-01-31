namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class BuildDefinitionReference
{
    public Project Project { get; set; }
    public BuildDefinition Definition { get; set; }
    public Repository Repository { get; set; }
}