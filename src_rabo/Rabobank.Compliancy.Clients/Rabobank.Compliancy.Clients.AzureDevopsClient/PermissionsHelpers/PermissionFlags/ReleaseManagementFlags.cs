namespace Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers.PermissionFlags;

/// <summary>
/// Bitflags enum for namespace ReleaseManagement with ID c788c23e-1b46-4162-8f5e-d7585343b5de
/// </summary>
[Flags]
public enum ReleaseManagementPermissionBits
{
    ViewReleaseDefinition = 1 << 0,
    EditReleaseDefinition = 1 << 1,
    DeleteReleaseDefinition = 1 << 2,
    ManageReleaseApprovers = 1 << 3,
    ManageReleases = 1 << 4,
    ViewReleases = 1 << 5,
    CreateReleases = 1 << 6,
    EditReleaseEnvironment = 1 << 7,
    DeleteReleaseEnvironment = 1 << 8,
    AdministerReleasePermissions = 1 << 9,
    DeleteReleases = 1 << 10,
    ManageDeployments = 1 << 11,
    ManageReleaseSettings = 1 << 12,
    ManageTaskHubExtension = 1 << 13
}