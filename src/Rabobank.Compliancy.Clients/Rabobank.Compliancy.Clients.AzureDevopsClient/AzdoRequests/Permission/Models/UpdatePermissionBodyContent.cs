namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;
/// <summary>
/// This is a Microsoft object used for serialization/deserialization
/// As Microsoft only provides the decommissioned WSDL entities and have not provided the new REST endpoint entities,
/// we have to provide our own.
/// </summary>
[Obsolete("This class is part of an undocumented legacy API.")]
public class UpdatePermissionBodyContent
{
    /// <summary>
    /// Json Serialized content of the <see cref="UpdatePermissionBody"/> class.
    /// </summary>
    public string? UpdatePackage { get; set; }
}