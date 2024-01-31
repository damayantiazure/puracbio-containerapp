using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Microsoft.VisualStudio.Services.Gallery.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Environment.Models;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;

/// <summary>
///     Provides methods to cater to all object needs from the Azure Devops API regarding
///     <see cref="EnvironmentInstance" />
/// </summary>
public interface IEnvironmentRepository
{
    /// <summary>
    ///     Gets all environments by projectId.
    /// </summary>
    /// <param name="organization">The organization the Project belongs to</param>
    /// <param name="projectId">The project the Environments belong to</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>
    ///     Nullable <see cref="EnvironmentInstance" /> representing a EnvironmentInstance (Environment) the way Azure
    ///     Devops API returns it.
    /// </returns>
    Task<IEnumerable<EnvironmentInstance>?> GetEnvironmentsAsync(string organization, Guid projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// </summary>
    /// <param name="organization">The organization the Project belongs to</param>
    /// <param name="scopeId">Id of the assigned scope</param>
    /// <param name="resourceId">Id of the resource on which the role is to be assigned</param>
    /// <param name="identityId"></param>
    /// <param name="content">
    ///     An object of type <see cref="PublisherUserRoleAssignmentRef" /> that represents
    ///     the data we will post in the message's body.
    /// </param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>
    ///     Nullable <see cref="RoleAssignmentBodyContent" /> representing the new role assignment.
    /// </returns>
    Task<PublisherRoleAssignment?> SetSecurityGroupsAsync(
        string organization, string scopeId, string resourceId, string identityId, RoleAssignmentBodyContent content,
        CancellationToken cancellationToken = default);
}