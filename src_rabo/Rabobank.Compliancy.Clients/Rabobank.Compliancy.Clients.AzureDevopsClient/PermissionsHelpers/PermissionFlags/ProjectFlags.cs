namespace Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers.PermissionFlags;

/// <summary>
/// Bitflags enum for namespace Project with ID 52d39943-cb85-4d7f-8fa8-c6baac873819
/// </summary>
[Flags]
public enum ProjectPermissionBits
{
    GENERIC_READ = 1 << 0,
    GENERIC_WRITE = 1 << 1,
    DELETE = 1 << 2,
    PUBLISH_TEST_RESULTS = 1 << 3,
    ADMINISTER_BUILD = 1 << 4,
    START_BUILD = 1 << 5,
    EDIT_BUILD_STATUS = 1 << 6,
    UPDATE_BUILD = 1 << 7,
    DELETE_TEST_RESULTS = 1 << 8,
    VIEW_TEST_RESULTS = 1 << 9,
    // Microsoft deleted the 10th bit, it no longer exists in the permission bits returned by the Azure Devops API.
    MANAGE_TEST_ENVIRONMENTS = 1 << 11,
    MANAGE_TEST_CONFIGURATIONS = 1 << 12,
    WORK_ITEM_DELETE = 1 << 13,
    WORK_ITEM_MOVE = 1 << 14,
    WORK_ITEM_PERMANENTLY_DELETE = 1 << 15,
    RENAME = 1 << 16,
    MANAGE_PROPERTIES = 1 << 17,
    MANAGE_SYSTEM_PROPERTIES = 1 << 18,
    BYPASS_PROPERTY_CACHE = 1 << 19,
    BYPASS_RULES = 1 << 20,
    SUPPRESS_NOTIFICATIONS = 1 << 21,
    UPDATE_VISIBILITY = 1 << 22,
    CHANGE_PROCESS = 1 << 23,
    AGILETOOLS_BACKLOG = 1 << 24
}