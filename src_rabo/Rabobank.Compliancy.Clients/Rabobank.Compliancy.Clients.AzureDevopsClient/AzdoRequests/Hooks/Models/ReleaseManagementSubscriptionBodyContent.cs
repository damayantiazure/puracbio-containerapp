namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Hooks.Models;

public class ReleaseManagementSubscriptionBodyContent
{
    public string? ProjectId { get; init; }
    public string? ReleaseDefinitionId { get; init; }
    public string? ReleaseEnvironmentId { get; init; }
    public string? ReleaseEnvironmentStatus { get; init; }
}
