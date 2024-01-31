using Rabobank.Compliancy.Domain.Compliancy.Authorizations;

namespace Rabobank.Compliancy.Infrastructure.Extensions;

internal static class IdentityExtensions
{
    internal static IList<IIdentity> NotExplicitDenied(this IEnumerable<IIdentity> allowedIdentities, IEnumerable<string> explicitDeniedIds)
    {
        var returnList = new List<IIdentity>();

        foreach (var allowedIdentity in allowedIdentities)
        {
            if (allowedIdentity is User && !explicitDeniedIds.Contains(allowedIdentity.UniqueId))
            {
                returnList.Add(allowedIdentity);
            }

            if (allowedIdentity is Group allowedIdentityAsGroup && !explicitDeniedIds.Contains(allowedIdentity.UniqueId))
            {
                var innerAllowedIdentitiesThatAreNotExplicitlyDenied = new[] { allowedIdentity }.NotExplicitDenied(explicitDeniedIds);
                allowedIdentityAsGroup.ClearMembers();
                allowedIdentityAsGroup.AddMembers(innerAllowedIdentitiesThatAreNotExplicitlyDenied);

                returnList.Add(allowedIdentityAsGroup);
            }
        }

        return returnList;
    }

    internal static bool GroupNameEquals(this string fullGroupName, string projectName, string groupNameToTestFor) =>
        $"[{projectName}]\\{groupNameToTestFor}".Equals(fullGroupName, StringComparison.OrdinalIgnoreCase)
        || groupNameToTestFor?.Equals(fullGroupName, StringComparison.OrdinalIgnoreCase) == true;
}