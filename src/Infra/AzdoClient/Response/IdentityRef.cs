using System;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

// Documentation: https://docs.microsoft.com/en-us/javascript/api/azure-devops-extension-api/identityref
public class IdentityRef
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; }
    public string UniqueName { get; set; }
}