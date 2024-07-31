using Microsoft.VisualStudio.Services.Security;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;

/// <summary>
/// Provides methods to cater to all object needs from the Azure Devops API regarding <see cref="AccessControlList"/>
/// </summary>
public interface IAccessControlListsRepository
{
    /// <summary>
    /// Gets all AccessControlLists for a specific project and security namespace.
    /// </summary>
    /// <param name="organization">The organization the AccessControlList belongs to</param>
    /// <param name="projectId">The project the AccessControlList belongs to</param>
    /// <param name="securityNamespaceId">The namespace of the AccessControlList</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable <see cref="AccessControlList"/> representing a AccessControlList (a list of permissions of users and groups withing that namespace) the way Azure Devops API returns it.</returns>
    Task<IEnumerable<AccessControlList>?> GetAccessControlListsForProjectAndSecurityNamespaceAsync(string organization, Guid projectId, Guid securityNamespaceId, CancellationToken cancellationToken = default);
}