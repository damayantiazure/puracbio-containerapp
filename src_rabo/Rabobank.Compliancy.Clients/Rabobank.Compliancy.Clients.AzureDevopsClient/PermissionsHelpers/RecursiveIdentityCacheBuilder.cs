using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Identity;
using Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using System.Collections.Concurrent;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers;

public class RecursiveIdentityCacheBuilder : IRecursiveIdentityCacheBuilder
{
    private readonly IIdentityRepository _identityRepository;
    private readonly SemaphoreSlim _concurrencyCacheBlocker = new(1, 1);

    private readonly ConcurrentDictionary<IdentityDescriptor, Identity> _cachedIdentities = new();

    public RecursiveIdentityCacheBuilder(IIdentityRepository identityRepository)
    {
        _identityRepository = identityRepository;
    }

    public async Task<IEnumerable<Identity>> GetIdentitiesFromCacheAsync(string organization, IEnumerable<IdentityDescriptor> identityDescriptors, CancellationToken cancellationToken = default)
    {
        var uncachedIdentities = identityDescriptors.Except(_cachedIdentities.Keys);

        if (uncachedIdentities.Any())
        {
            await BuildIdentityCacheRecursiveAsync(organization, uncachedIdentities, cancellationToken);
        }

        return identityDescriptors.Select(descriptor => _cachedIdentities[descriptor]);
    }

    private async Task BuildIdentityCacheRecursiveAsync(string organization, IEnumerable<IdentityDescriptor> identityDescriptors, CancellationToken cancellationToken = default)
    {
        await _concurrencyCacheBlocker.WaitAsync(cancellationToken);
        try
        {
            // A hashset is made to keep track of what identities still need to be fetched. At first, this is all of them.
            // We use a hashset, because multiple groups can have the same member, and we only want to fetch it once.
            var newFetchList = new HashSet<IdentityDescriptor>(identityDescriptors);

            // While the newFetchList has any, we do another iteration to get their identities.
            while (newFetchList.Count > 0)
            {
                var fetchedIdentities = await _identityRepository.GetIdentitiesForIdentityDescriptorsAsync(organization, newFetchList, QueryMembership.Direct, cancellationToken);

                // After fetching the identities (which are placed in a seperate enumerable) the newFetchList is cleared.
                // We just fetched them and we don't want to fetch the same identities again.
                newFetchList.Clear();
                if (fetchedIdentities == null)
                {
                    continue;
                }

                // We first add all fetched identities to the cache. If we do it in the loop later, we might save some iterations,
                // but it might add items to the newFetchList that have already been fetched. This could happen if fetchedIdentities
                // contains users and groups and these groups contain the same users.
                fetchedIdentities.ForEach(f => _cachedIdentities.TryAdd(f.Descriptor, f));

                // And then check if any of the identities are groups which contain identities we need to fetch.
                // If so, add them to newFetchList, so the next iteration of the while loop will fetch them and
                // add them to the cache.
                foreach (var fetchedIdentity in fetchedIdentities)
                {
                    if (fetchedIdentity.IsContainer)
                    {
                        newFetchList.UnionWith(fetchedIdentity.Members.Where(member => !_cachedIdentities.ContainsKey(member)));
                    }
                }
            }
        }
        finally
        {
            _concurrencyCacheBlocker.Release();
        }
    }
}