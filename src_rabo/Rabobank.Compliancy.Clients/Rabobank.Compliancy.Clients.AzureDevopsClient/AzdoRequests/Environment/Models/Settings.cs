namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Environment.Models;

/// <summary>
/// This class is not documented in Microsoft's resources. Attempted to locate it in alternative packages,
/// but unfortunately, it remained elusive.
/// </summary>

public class CheckSettings
{
    /// <summary>
    /// Name of check
    /// </summary>
    public string? DisplayName { get; set; }
    /// <summary>
    /// Duration between retry attempts in case the initial invocation of the Azure Function fails
    /// </summary>
    public int RetryInterval { get; set; }
    /// <summary>
    /// Related to variable groups and their usage in pipeline tasks
    /// </summary>
    public string? LinkedVariableGroup { get; set; }
    public SettingsInputs Inputs { get; set; } = new();
    public SettingsDefinitionRef DefinitionRef { get; set; } = new();
}