namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class ReleaseDefinitionGatesStep
{
    public ReleaseDefinitionGatesOptions GatesOptions { get; set; }
    public ReleaseDefinitionGate[] Gates { get; set; }
}