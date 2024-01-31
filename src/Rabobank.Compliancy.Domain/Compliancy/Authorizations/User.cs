#nullable enable

namespace Rabobank.Compliancy.Domain.Compliancy.Authorizations;

public class User : IIdentity
{
    public User(string displayName, string uniqueId)
    {
        DisplayName = displayName;
        UniqueId = uniqueId;
    }

    public string? MailAddress { get; set; }
    public string DisplayName { get; }
    public string UniqueId { get; }

    public IEnumerable<IIdentity> MapIdentitiesHierarchy()
    {
        return new[] { this };
    }
}