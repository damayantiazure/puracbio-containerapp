#nullable enable

using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Enums;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Application.Security;

/// <inheritdoc/>
public class CheckAuthorizationProcess : ICheckAuthorizationProcess
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IProjectService _projectService;

    public CheckAuthorizationProcess(IAuthorizationService authorizationService, IProjectService projectService)
    {
        _authorizationService = authorizationService;
        _projectService = projectService;
    }

    public async Task<bool> IsAuthorized(AuthorizationRequest authorizationRequest, AuthenticationHeaderValue? authenticationHeaderValue, CancellationToken cancellationToken = default)
    {
        if (authenticationHeaderValue == null)
        {
            return false;
        }

        var user = await _authorizationService.GetCurrentUserAsync(authorizationRequest.Organization, authenticationHeaderValue, cancellationToken);

        var permissions = await _authorizationService.GetPermissionsForUserOrGroupAsync(authorizationRequest.Organization,
            authorizationRequest.ProjectId, user.Id, cancellationToken);

        return IsUserAllowedToManageProjectProperties(permissions);
    }

    private static bool IsUserAllowedToManageProjectProperties(IEnumerable<Permission> permissions) =>
        permissions.Any(p => p.Name == Domain.Constants.PermissionConstants.ManageProjectProperties &&
                             (p.Type == PermissionType.Allow || p.Type == PermissionType.AllowInherited));

    /// <inheritdoc/>
    public async Task<UserPermission?> GetUserPermissionAsync(AuthorizationRequest authorizationRequest, AuthenticationHeaderValue authenticationHeaderValue, CancellationToken cancellationToken = default)
    {
        var project = await _projectService.GetProjectByIdAsync(authorizationRequest.Organization, authorizationRequest.ProjectId, cancellationToken);
        var userId = (await _authorizationService.GetCurrentUserAsync(project.Organization, authenticationHeaderValue, cancellationToken)).Id;

        return await _authorizationService.GetUserPermissionsAsync(project, userId, cancellationToken);
    }
}