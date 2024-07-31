using Rabobank.Compliancy.Clients.LogAnalyticsClient.Requests.Authentication.Models;
using Rabobank.Compliancy.Clients.LogAnalyticsClient.Requests.Workspace.Models;

namespace Rabobank.Compliancy.Clients.LogAnalyticsClient.Repositories.Interfaces;

/// <summary>
/// Provides methods to cater to all object needed to communicate to loganalytics.
/// </summary>
public interface ILogAnalyticsRepository
{
    /// <summary>
    /// QueryWorkspaceAsync will perform a kusto query language (KQL) to retrieve loganalytics logs.
    /// </summary>
    /// <param name="kustoQuery">The kusto query language that is presented as a <see cref="string"/>.</param>
    /// <param name="authentication">An instance of the <see cref="Authentication"/> that contains the bearer token.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A nullable instance of <see cref="WorkspaceResponse"/> response.</returns>
    Task<WorkspaceResponse?> QueryWorkspaceAsync(string kustoQuery, Authentication authentication, CancellationToken cancellationToken = default);

    /// <summary>
    /// GetAuthenticationAsync will retrieve a new authentication token.
    /// </summary>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A nullable instance of <see cref="WorkspaceResponse"/> response that contains the bearer token.</returns>
    Task<Authentication?> GetAuthenticationAsync(CancellationToken cancellationToken = default);
}