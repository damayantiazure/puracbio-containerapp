namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.WorkItemTracking.Models;

/// <summary>
/// This is a Microsoft object used for serialization/deserialization
/// As Microsoft only provides the decommissioned WSDL entities and have not provided the new REST endpoint entities,
/// we have to provide our own.
/// </summary>
public class GetQueryBodyContent
{
    /// <summary>
    /// The text of the WIQL query
    /// </summary>
    public string? Query { get; init; }
}
