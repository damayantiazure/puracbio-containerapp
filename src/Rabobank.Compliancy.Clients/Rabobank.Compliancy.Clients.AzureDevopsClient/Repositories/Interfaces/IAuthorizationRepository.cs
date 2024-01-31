using Microsoft.VisualStudio.Services.Location;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;

public interface IAuthorizationRepository
{
    /// <summary>
    /// Gets a gitRepo by ID.
    /// </summary>
    /// <param name="authenticationHeaderValue">The custom header contains the authentication information of the frontend user</param>
    /// <param name="organization">The organization the connectiondata belongs to</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns><see cref="ConnectionData"/> representing the data needed for an Azure DevOps connection.</returns>
    Task<ConnectionData> GetUserForAccessToken(AuthenticationHeaderValue authenticationHeaderValue, string organization, CancellationToken cancellationToken = default);
}