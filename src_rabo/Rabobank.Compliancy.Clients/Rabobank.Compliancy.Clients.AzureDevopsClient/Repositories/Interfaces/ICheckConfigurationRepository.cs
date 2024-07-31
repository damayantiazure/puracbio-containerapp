using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Environment.Models;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;

/// <summary>
///     Provides methods to cater to all object needs from the Azure Devops API regarding <see cref="CheckConfiguration" />
/// </summary>
public interface ICheckConfigurationRepository
{
    /// <summary>
    ///     Gets all environments by projectId.
    /// </summary>
    /// <param name="organization">The organization the Project belongs to</param>
    /// <param name="projectId">The project the Environments belong to</param>
    /// <param name="environmentId">The environment the checks belong to</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>
    ///     Nullable enumerable of <see cref="CheckConfiguration" /> representing a collection of CheckConfigurations
    ///     (checks) the way Azure Devops API returns it.
    /// </returns>
    Task<IEnumerable<CheckConfiguration>?> GetCheckConfigurationsForEnvironmentAsync(string organization,
        Guid projectId, int environmentId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a check with the details as specified in the content parameter.
    /// </summary>
    /// <param name="organization">The organization the Project belongs to</param>
    /// <param name="projectId">The project the Environments belong to</param>
    /// <param name="content">Details about the check to be created.</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>
    ///     Nullable instance of <see cref="CheckConfiguration" /> representing a CheckConfigurations
    ///     (checks) the way Azure Devops API returns it.
    /// </returns>
    Task<CheckConfiguration?> CreateCheckForEnvironmentAsync(
        string organization, Guid projectId, EnvironmentCheckBodyContent content,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Delete the check with the specified id.
    /// </summary>
    /// <param name="organization">The organization the Project belongs to</param>
    /// <param name="projectId">The project the Environments belong to</param>
    /// <param name="id">Id of the check to delete.</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    Task DeleteCheckForEnvironmentAsync(
        string organization, Guid projectId, string id,
        CancellationToken cancellationToken = default);
}