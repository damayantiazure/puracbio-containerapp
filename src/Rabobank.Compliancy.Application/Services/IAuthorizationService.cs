#nullable enable
using Rabobank.Compliancy.Domain.Compliancy;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Application.Services;

/// <summary>
/// An interface defining a process for authorizations and permissions.
/// </summary>
public interface IAuthorizationService
{
    Task<User> GetCurrentUserAsync(string organization, AuthenticationHeaderValue authenticationHeaderValue, CancellationToken cancellationToken = default);

    Task<IEnumerable<Permission>> GetPermissionsForUserOrGroupAsync(string organization, Guid projectId, Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// GetUserPermissionsAsync will check if user is allowed to edit permissions.
    /// </summary>
    /// <param name="project">The project instance to be used.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Returns new instance of the <see cref="UserPermission"/> class.</returns>
    Task<UserPermission?> GetUserPermissionsAsync(Project project, Guid userId, CancellationToken cancellationToken = default);
}