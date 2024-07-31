namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

// Documentation: https://docs.microsoft.com/en-us/javascript/api/azure-devops-extension-api/identityrefwithvote
public class IdentityRefWithVote
{
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public string UniqueName { get; set; }
    public bool IsContainer { get; set; }
    public int Vote { get; set; }
}