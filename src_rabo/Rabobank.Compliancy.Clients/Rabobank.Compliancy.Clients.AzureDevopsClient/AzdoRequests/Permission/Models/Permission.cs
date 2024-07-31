namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;

[Obsolete("This class is part of an undocumented legacy API.")]
public class Permission
{
    public int PermissionBit { get; set; }
    public string? DisplayName { get; set; }
    public int PermissionId { get; set; }
    public string? PermissionDisplayString { get; set; }
    public Guid? NamespaceId { get; set; }
    public string? PermissionToken { get; set; }
    public int ExplicitPermissionId { get; set; }
    public int OriginalPermissionId { get; set; }
    public bool CanEdit { get; set; }
    public bool InheritDenyOverride { get; set; }
    public bool DisplayTrace { get; set; }
}