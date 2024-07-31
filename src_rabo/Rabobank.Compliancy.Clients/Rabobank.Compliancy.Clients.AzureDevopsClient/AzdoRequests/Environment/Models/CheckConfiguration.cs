namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Environment.Models;

/// <summary>
/// Representation of the CheckConfiguration from Azure Devops API 
/// "{organization}/{project}/_apis/pipelines/checks/configurations?resourceType={resourceType}&resourceId={resourceId}"
/// 
/// This class exists in Microsoft.Azure.Pipelines.Policy.Client, but that package is .Net Framework 4.7.1 and has been in preview for 2 years.
/// It's still being updated regularly, so keep an eye on it.
/// </summary>
public class CheckConfiguration
{
    public CheckSettings? Settings { get; set; }
}