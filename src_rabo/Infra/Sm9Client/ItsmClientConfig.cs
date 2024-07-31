namespace Rabobank.Compliancy.Infra.Sm9Client;

public class ItsmClientConfig
{
    public ItsmClientConfig(string endpoint, string resource, string managedIdentityClientId)
    {
        Endpoint = endpoint;
        if (!Guid.TryParse(resource, out _))
        {
            throw new ArgumentOutOfRangeException(nameof(resource), $"{nameof(resource)} is not a valid Guid");
        }
        Resource = resource;
        ManagedIdentityClientId = managedIdentityClientId;
    }

    public string Endpoint { get; }
    public string Resource { get; }
    public string ManagedIdentityClientId { get; }
}