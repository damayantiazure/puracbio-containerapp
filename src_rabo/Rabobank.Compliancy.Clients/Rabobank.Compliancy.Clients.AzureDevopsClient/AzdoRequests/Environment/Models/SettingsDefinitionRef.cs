namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Environment.Models;

public class SettingsDefinitionRef
{
    /// <summary>
    /// Identifier of check like invoke azure function, business hours etc...
    /// </summary>
    public string? Id { get; set; }
    /// <summary>
    /// Name of check
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// Version of check
    /// </summary>
    public string? Version { get; set; }
}