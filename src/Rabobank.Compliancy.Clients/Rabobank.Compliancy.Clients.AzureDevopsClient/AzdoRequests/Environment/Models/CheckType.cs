namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Environment.Models;

/// <summary>
/// This class exists in Microsoft.Azure.Pipelines.Policy.Client, but that package is .Net Framework 4.7.1 and has been in preview for 2 years.
/// It's still being updated regularly, so keep an eye on it.
/// for more information check https://www.nuget.org/packages/Microsoft.Azure.Pipelines.Policy.Client/19.207.0-preview#supportedframeworks-body-tab
/// </summary>
public class CheckType
{
    /// <summary>
    /// Identifier of check
    /// </summary>
    public string? Id { get; set; }
    /// <summary>
    /// Name of check
    /// </summary>
    public string? Name { get; set; }
}