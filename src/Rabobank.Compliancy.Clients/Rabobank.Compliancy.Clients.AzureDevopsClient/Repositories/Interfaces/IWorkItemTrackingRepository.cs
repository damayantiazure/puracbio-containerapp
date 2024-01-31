using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.WorkItemTracking.Models;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;

/// <summary>
/// Provides methods to cater to all object needs from the Azure Devops API regarding <see cref="WorkItemTrackingRepository"/>
/// </summary>
public interface IWorkItemTrackingRepository
{
    /// <summary>
    /// Gets the results of the query given its WIQL
    /// </summary>
    /// <param name="organization">The organization the WorkItemTracking belongs to</param>
    /// <param name="projectId">The project the WorkItemTracking belongs to</param>
    /// <param name="team">Team ID or team name</param>
    /// <param name="query">The WIQL query</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable <see cref="WorkItemQueryResult"/> representing a WorkItemQueryResult the way Azure Devops API returns it.</returns>
    Task<WorkItemQueryResult?> GetQueryByWiqlAsync(string organization, Guid projectId, string team, GetQueryBodyContent query, CancellationToken cancellationToken = default);
}