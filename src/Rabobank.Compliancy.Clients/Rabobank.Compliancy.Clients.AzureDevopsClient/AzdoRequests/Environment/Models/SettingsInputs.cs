namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Environment.Models;

public class SettingsInputs
{
    /// <summary>
    /// HTTP Request methods like POST, GET, etc
    /// </summary>
    public string? Method { get; set; }
    /// <summary>
    /// URL of the Azure function that needs to be invoked​.
    /// </summary>
    public string? Function { get; set; }
    /// <summary>
    /// Waiting a decision from resource.
    /// </summary>
    public bool? WaitForCompletion { get; set; }
    /// <summary>
    /// Function or Host key with which to access this function.
    /// </summary>
    public string? Key { get; set; }
}