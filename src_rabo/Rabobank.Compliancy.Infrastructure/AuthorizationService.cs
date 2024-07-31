#nullable enable

using Microsoft.VisualStudio.Services.Location;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Constants;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using System.Net;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Infrastructure;

/// <inheritdoc/>
public class AuthorizationService : IAuthorizationService
{
    private readonly IAuthorizationRepository _authorizationRepository;
    private readonly IPermissionRepository _permissionsRepository;

    private readonly Dictionary<string, User> _usersPerToken = new();
    private readonly Dictionary<Guid, IEnumerable<Permission>> _permissionsPerUserGroup = new();
    private const string _tokenNotValidOrExpiredError = "Provided token is not valid or expired.";

    public AuthorizationService(IAuthorizationRepository authorizationRepository,
        IPermissionRepository permissionRepository)
    {
        _authorizationRepository = authorizationRepository;
        _permissionsRepository = permissionRepository;
    }

    public Task<User> GetCurrentUserAsync(string organization, AuthenticationHeaderValue authenticationHeaderValue,
        CancellationToken cancellationToken = default)
    {
        if (authenticationHeaderValue.Parameter == null)
        {
            throw new ArgumentNullException(authenticationHeaderValue.Parameter);
        }

        return GetCurrentUserInternalAsync(organization, authenticationHeaderValue, cancellationToken);
    }

    private async Task<User> GetCurrentUserInternalAsync(string organization,
        AuthenticationHeaderValue authenticationHeaderValue, CancellationToken cancellationToken = default)
    {
        if (_usersPerToken.TryGetValue(authenticationHeaderValue.Parameter!, out var cachedUser))
        {
            return cachedUser;
        }

        ConnectionData? authorizationData = null;

        try
        {
            authorizationData =
                await _authorizationRepository.GetUserForAccessToken(authenticationHeaderValue, organization,
                    cancellationToken);
        }
        // Sometimes Azure DevOps returns a signin page if token is not valid or expired. 
        // The Azure DevOps client will try to serialize this to an object but that fails because the response message
        // is not json but html.
        catch (UnsupportedMediaTypeException)
        {
            throw new TokenInvalidException(_tokenNotValidOrExpiredError);
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new TokenInvalidException($"{_tokenNotValidOrExpiredError} {ex.Message}");
            }
        }

        // An argumentException is not mea`Cnt for this situation. As this code is refactored with the intention
        // to have the current functionality unchanged it is left this way, but need to change in the future
        // when a common solution for validation/exception handling is introduced.
        if (authorizationData?.AuthorizedUser?.Id == null || authorizationData.AuthorizedUser.Id == Guid.Empty)
        {
            throw new ArgumentException("Unable to retrieve user.");
        }

        var user = new User
        {
            Id = authorizationData.AuthorizedUser!.Id,
            Username = authorizationData.AuthorizedUser!.Properties?.Values?.FirstOrDefault()?.ToString()
        };

        RemoveOldTokensFromCache(user.Id);

        _usersPerToken.Add(authenticationHeaderValue.Parameter!, user);

        return _usersPerToken[authenticationHeaderValue.Parameter!];
    }

    public async Task<IEnumerable<Permission>> GetPermissionsForUserOrGroupAsync(string organization, Guid projectId,
        Guid id, CancellationToken cancellationToken = default)
    {
        if (_permissionsPerUserGroup.TryGetValue(id, out var cachedPermission))
        {
            return cachedPermission;
        }

        var permissionData = await _permissionsRepository.GetPermissionsUserOrGroupAsync(organization, projectId, id,
            cancellationToken);

        var securityData = permissionData?.Security;
        if (securityData?.Permissions == null || string.IsNullOrEmpty(securityData.DescriptorIdentifier) ||
            securityData.DescriptorIdentifier.Contains(PermissionConstants.ConflictSecurityDescriptor))
        {
            return Enumerable.Empty<Permission>();
        }

        var permissions = new List<Permission>();
        foreach (var permission in securityData.Permissions)
        {
            permissions.Add(new Permission
            {
                Type = MapPermissionType(permission.PermissionId, permission.DisplayName),
                Name = permission.DisplayName
            });
        }

        _permissionsPerUserGroup.Add(id, permissions);

        return permissions;
    }

    private void RemoveOldTokensFromCache(Guid userId)
    {
        foreach (var kvp in _usersPerToken.Where(kvp => kvp.Value.Id == userId))
        {
            _usersPerToken.Remove(kvp.Key);
        }
    }

    private static PermissionType MapPermissionType(int permissionId, string? displayName)
    {
        return permissionId switch
        {
            0 => PermissionType.NotSet,
            1 => PermissionType.Allow,
            2 => PermissionType.Deny,
            3 => PermissionType.AllowInherited,
            4 => PermissionType.DenyInherited,
            5 => PermissionType.AllowSystem,
            _ => throw new InvalidOperationException(
                $"Permission id {permissionId} with display name {displayName} cannot be mapped.")
        };
    }

    /// <inheritdoc/>
    public async Task<UserPermission?> GetUserPermissionsAsync(Project project, Guid userId,
        CancellationToken cancellationToken = default)
    {
        var permissionsProjectId =
            await _permissionsRepository.GetPermissionsUserOrGroupAsync(project.Organization, project.Id, userId,
                cancellationToken);

        if (permissionsProjectId == null)
        {
            return null;
        }

        var identity = permissionsProjectId.Identity;

        var user = new User
        {
            Id = identity.TeamFoundationId,
            MailAddress = identity.MailAddress,
            Username = identity.DisplayName
        };

        return new UserPermission(user)
        {
            IsAllowedToEditPermissions = permissionsProjectId.Security != null &&
                                         permissionsProjectId.Security.CanEditPermissions
        };
    }
}