using Microsoft.VisualStudio.Services.Identity;
using Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers.Interfaces;
using Rabobank.Compliancy.Domain.Compliancy.Authorizations;

namespace Rabobank.Compliancy.Infrastructure.Parsers;

internal class IdentityParser
{
    private readonly IRecursiveIdentityCacheBuilder _recursiveIdentityCacheBuilder;

    public IdentityParser(IRecursiveIdentityCacheBuilder recursiveIdentityCacheBuilder)
    {
        _recursiveIdentityCacheBuilder = recursiveIdentityCacheBuilder ?? throw new ArgumentNullException(nameof(recursiveIdentityCacheBuilder));
    }

    public async Task<IEnumerable<IIdentity>> ParseIdentityDescriptors(string organization, IEnumerable<IdentityDescriptor> identityDescriptors, CancellationToken cancellationToken = default)
    {
        if (identityDescriptors == null || !identityDescriptors.Any())
        {
            return Enumerable.Empty<IIdentity>();
        }

        var azdoIdentities = await _recursiveIdentityCacheBuilder.GetIdentitiesFromCacheAsync(organization, identityDescriptors, cancellationToken);
        var domainIdentities = new List<IIdentity>();

        foreach (var identity in azdoIdentities)
        {
            domainIdentities.Add(IsGroup(identity)
                ? await CreateGroupAsync(organization, identity, cancellationToken)
                : ParseUser(identity));
        }
        return domainIdentities;
    }

    private static bool IsGroup(Identity identity)
    {
        return identity.Descriptor.IsTeamFoundationType();
    }

    private async Task<IIdentity> CreateGroupAsync(string organization, Identity identity, CancellationToken cancellationToken = default)
    {
        var domainGroup = new Group(identity.DisplayName, identity.Descriptor.ToString());
        if (identity.Members != null && identity.Members.Any())
        {
            var members = await GetMembersAsync(organization, identity, cancellationToken);
            foreach (var member in members)
            {
                domainGroup.AddMember(member);
            }
        }

        return domainGroup;
    }

    private async Task<IEnumerable<IIdentity>> GetMembersAsync(string organization, Identity identity, CancellationToken cancellationToken = default)
    {
        return await ParseIdentityDescriptors(organization, identity.Members.Select(mem => mem), cancellationToken);
    }

    private static IIdentity ParseUser(Identity identity)
    {
        var uniqueId = identity.Descriptor.ToString();
        var displayName = GetDisplayName(identity, identity.Descriptor.IsClaimsIdentityType());
        return new User(displayName, uniqueId);
    }

    private static string GetDisplayName(Identity identity, bool isAadClaimsUser)
    {
        if (!isAadClaimsUser)
        {
            return identity.DisplayName;
        }
        var displayName = identity.Properties["Account"].ToString();
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = identity.DisplayName;
        }

        return displayName;
    }
}