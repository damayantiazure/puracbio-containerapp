namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Environment.Models;

public class EnvironmentCheckBodyContent
{
    public CheckType? Type { get; set; }
    public CheckSettings? Settings { get; set; }
    public Resource? Resource { get; set; }
    public int Timeout { get; set; }
}