#nullable enable

namespace Rabobank.Compliancy.Domain.Compliancy;

public class User
{
    /// <summary>
    /// Getter and setter of the user identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Getter and setter of the name of the user.
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    /// Getter and setter of the email address of the user.
    /// </summary>
    public string? MailAddress { get; init; }
}