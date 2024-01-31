#nullable enable

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

// Documentation: https://docs.microsoft.com/en-us/javascript/api/azure-devops-extension-api/identity
public class Identity
{
    public string? Id { get; set; }
    public string? Descriptor { get; set; }
    public string? SubjectDescriptor { get; set; }
    public string? ProviderDisplayName { get; set; }
    public bool IsActive { get; set; }
    public IdentityProperties? Properties { get; set; }
}