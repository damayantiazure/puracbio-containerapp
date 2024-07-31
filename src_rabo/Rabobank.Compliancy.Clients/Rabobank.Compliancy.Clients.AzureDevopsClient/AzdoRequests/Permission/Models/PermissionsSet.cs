namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;

/// <summary>
/// This is a Microsoft object used for serialization/deserialization
/// As Microsoft only provides the decommissioned WSDL entities and have not provided the new REST endpoint entities,
/// we have to provide our own.
/// </summary>
[Obsolete("This class is part of an undocumented legacy API.")]
public class PermissionsSet
{
    /// <summary>
    /// Getter and setter for the 'CanEditPermissions' property that results whether the user has edit permissions.
    /// </summary>
    public bool CanEditPermissions { get; init; }
    public IEnumerable<Permission>? Permissions { get; set; }
    public Guid? CurrentTeamFoundationId { get; set; }
    public string? DescriptorIdentifier { get; set; }
    public string? DescriptorIdentityType { get; set; }
    public bool IsAbleToEditAtLeastOnePermission { get; set; }
    public bool AreExplicitPermissionsSet { get; set; }
    public bool IsEligibleForRemove { get; set; }
    public bool AutoGrantCurrentIdentity { get; set; }
    public bool IsAadGroup { get; set; }
}