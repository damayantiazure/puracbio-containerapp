namespace Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers.PermissionFlags;

/// <summary>
/// Bitflags enum for namespace GitRepo with ID 2e9eb7ed-3c0a-47d4-87c1-0ffdd275fd87
/// </summary>
[Flags]
public enum GitRepositoryPermissionBits
{
    Administer = 1 << 0,
    GenericRead = 1 << 1,
    GenericContribute = 1 << 2,
    ForcePush = 1 << 3,
    CreateBranch = 1 << 4,
    CreateTag = 1 << 5,
    ManageNote = 1 << 6,
    PolicyExempt = 1 << 7,
    CreateRepository = 1 << 8,
    DeleteRepository = 1 << 9,
    RenameRepository = 1 << 10,
    EditPolicies = 1 << 11,
    RemoveOthersLocks = 1 << 12,
    ManagePermissions = 1 << 13,
    PullRequestContribute = 1 << 14,
    PullRequestBypassPolicy = 1 << 15,
    ViewAdvSecAlerts = 1 << 16,
    DismissAdvSecAlerts = 1 << 17,
    ManageAdvSecScanning = 1 << 18
}