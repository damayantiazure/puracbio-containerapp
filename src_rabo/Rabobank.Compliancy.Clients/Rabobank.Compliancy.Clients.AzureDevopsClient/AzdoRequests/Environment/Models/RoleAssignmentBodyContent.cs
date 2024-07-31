namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Environment.Models;

public class RoleAssignmentBodyContent
{
    /// <summary>
    /// The name of the role assigned.
    /// </summary>
    public string? RoleName { get; set; }
    /// <summary>
    /// Identifier of the user given the role assignment.
    /// </summary>
    public string? UniqueName { get; set; }
    /// <summary>
    /// Unique id of the user given the role assignment.
    /// </summary>
    public string? UserId { get; set; }
}