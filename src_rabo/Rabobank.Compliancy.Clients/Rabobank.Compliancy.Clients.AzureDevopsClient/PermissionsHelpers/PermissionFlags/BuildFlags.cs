namespace Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers.PermissionFlags;

/// <summary>
/// Bitflags enum for namespace Build with ID 33344d9c-fc72-4d6f-aba5-fa317101a7e9
/// </summary>
[Flags]
public enum BuildPermissionBits
{
    ViewBuilds = 1 << 0,
    EditBuildQuality = 1 << 1,
    RetainIndefinitely = 1 << 2,
    DeleteBuilds = 1 << 3,
    ManageBuildQualities = 1 << 4,
    DestroyBuilds = 1 << 5,
    UpdateBuildInformation = 1 << 6,
    QueueBuilds = 1 << 7,
    ManageBuildQueue = 1 << 8,
    StopBuilds = 1 << 9,
    ViewBuildDefinition = 1 << 10,
    EditBuildDefinition = 1 << 11,
    DeleteBuildDefinition = 1 << 12,
    OverrideBuildCheckInValidation = 1 << 13,
    AdministerBuildPermissions = 1 << 14
}