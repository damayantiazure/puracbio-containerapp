#nullable enable

namespace Rabobank.Compliancy.Domain.Compliancy;

public class UserPermission
{
    public UserPermission(User user)
    {
        User = user;
    }

    /// <summary>
    /// Getter and setter to verify if the user has edit permissions.
    /// </summary>
    public bool IsAllowedToEditPermissions { get; init; }

    /// <summary>
    /// Getter for the user instance.
    /// </summary>
    public User User { get; }
}