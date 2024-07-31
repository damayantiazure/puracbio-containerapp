namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class EnvironmentSecurityGroup
{
    public IdentityRef Identity { get; set; }
    public EnvironmentSecurityRole Role { get; set; }
    public string Access { get; set; }
    public string AccessDisplayName { get; set; }
}