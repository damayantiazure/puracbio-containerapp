namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;
/// <summary>
/// This is a Microsoft object used for serialization/deserialization
/// As Microsoft only provides the decommissioned WSDL entities and have not provided the new REST endpoint entities,
/// we have to provide our own.
/// </summary>
[Obsolete("This class is part of an undocumented legacy API.")]
public class UpdatePermissionEntity
{
    public string? Token { get; set; }
    public int? PermissionId { get; set; }
    public string? NamespaceId { get; set; }
    public int? PermissionBit { get; set; }
}