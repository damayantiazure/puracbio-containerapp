using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class DefinitionReference
{
    public Branch Branch { get; set; }
    public Version Version { get; set; }
    public IsTriggeringArtifact IsTriggeringArtifact { get; set; }
    public Project Project { get; set; }            
    public Definition Definition { get; set; }
}