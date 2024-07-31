namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;
/// <summary>
/// This is a Microsoft object used for serialization/deserialization
/// As Microsoft only provides the decommissioned WSDL entities and have not provided the new REST endpoint entities,
/// we have to provide our own.
/// </summary>
[Obsolete("This class is part of an undocumented legacy API.")]
public class UpdatePermissionBody
{
    public Guid TeamFoundationId { get; set; }
    public Guid PermissionSetId { get; set; }
    public string? PermissionSetToken { get; set; }
    public string? DescriptorIdentityType { get; set; }
    public string? DescriptorIdentifier { get; set; }
    public bool RefreshIdentities { get; set; }
    public bool IsRemovingIdentity { get; set; }
    public string? TokenDisplayName { get; set; }
    public IList<UpdatePermissionEntity> Updates { get; set; } = new List<UpdatePermissionEntity>();
}