namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.DistributedTask.Models;

/// <summary>
/// This is a Microsoft object used for serialization/deserialization
/// As Microsoft only provides the decommissioned WSDL entities and have not provided the new REST endpoint entities,
/// we have to provide our own.
/// </summary>
public class AddTaskEventBodyContent
{
    /// <summary>
    /// The ID of the pipeline job affected by the event.
    /// </summary>
    public string? JobId { get; init; }

    /// <summary>
    /// The name of the pipeline job event.
    /// </summary>
    public string? Name { get; init; }
}
