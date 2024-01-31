#nullable enable

using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Domain.Compliancy;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Application.Interfaces;

/// <summary>
/// An interface that implements a process for validating the user authorizations and permissions.
/// </summary>
public interface ICheckAuthorizationProcess
{
    /// <summary>
    /// IsAuthorized will check if user is authorized to continue with
    /// </summary>
    /// <param name="authorizationRequest">The authorization request containing the project details.</param>
    /// <param name="authenticationHeaderValue">The header value that contains the bearer token.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A <see cref="bool"/> whether the user is authorized.</returns>
    Task<bool> IsAuthorized(AuthorizationRequest authorizationRequest, AuthenticationHeaderValue? authenticationHeaderValue, CancellationToken cancellationToken = default);

    /// <summary>
    /// UserHasEditPermissionsAsync will retrieve the user and check if this user has edit permission in a project.
    /// </summary>
    /// <param name="authorizationRequest">The authorization request containing the project details.</param>
    /// <param name="authenticationHeaderValue">The header value that contains the bearer token.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Returns a new instance of the <see cref="UserPermission"/> class.</returns>
    Task<UserPermission?> GetUserPermissionAsync(AuthorizationRequest authorizationRequest, AuthenticationHeaderValue authenticationHeaderValue, CancellationToken cancellationToken = default);
}