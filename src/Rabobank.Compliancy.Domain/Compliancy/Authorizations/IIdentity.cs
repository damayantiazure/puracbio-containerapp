namespace Rabobank.Compliancy.Domain.Compliancy.Authorizations;

/// <summary>
/// Represents anything that can be an Identity in our domain. 
/// Identities should have an ID, a name and a way to expose their members if they have any.
/// </summary>
public interface IIdentity
{
    string DisplayName { get; }
    string UniqueId { get; }
    IEnumerable<IIdentity> MapIdentitiesHierarchy();
}