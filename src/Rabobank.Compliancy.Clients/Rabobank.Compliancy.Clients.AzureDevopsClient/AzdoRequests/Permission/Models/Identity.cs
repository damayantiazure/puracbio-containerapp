namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;

[Obsolete("This class is part of an undocumented legacy API.")]
public class Identity
{
    /// <summary>
    /// Getter and setter for the user identifier.
    /// </summary>
    public Guid TeamFoundationId { get; init; }

    /// <summary>
    /// Getter and setter for the mail address of the user.
    /// </summary>
    public string? MailAddress { get; init; }

    /// <summary>
    /// Getter and setter for the display name of the user.
    /// </summary>
    public string? DisplayName { get; init; }
}