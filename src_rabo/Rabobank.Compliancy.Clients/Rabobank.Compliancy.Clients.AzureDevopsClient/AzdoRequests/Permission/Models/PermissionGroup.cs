namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;

/// <summary>
/// This is a Microsoft object used for serialization/deserialization
/// As Microsoft only provides the decommissioned WSDL entities and have not provided the new REST endpoint entities,
/// we have to provide our own.
/// </summary>
[Obsolete("This class is part of an undocumented legacy API.")]
public class PermissionGroup
{
    public string? FriendlyDisplayName { get; init; }
    public string? DisplayName { get; init; }
    public Guid TeamFoundationId { get; init; }
    public string? Description { get; init; }
    public string? IdentityType { get; init; }
    public bool IsProjectLevel { get; init; }
}