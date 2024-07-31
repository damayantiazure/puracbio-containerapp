namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

// Documentation: https://docs.microsoft.com/en-us/javascript/api/azure-devops-extension-api/connectiondata
public class ConnectionData
{
    public Identity AuthenticatedUser { get; set; }
    public Identity AuthorizedUser { get; set; }
}