using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Rabobank.Compliancy.Infra.AzdoClient.Extensions;

public static class MemoryCacheExtensions
{
    private const int DefaultCacheExpirationInSeconds = 60;

    public static Task<T> GetOrCreateAsync<T>(this IMemoryCache cache, IAzdoRestClient client, IAzdoRequest<T> request, string organization = null) where T : new()
    {
        organization ??= client.GetOrganization();
        return cache.GetOrCreateAsync(request.Url(organization).ToString(),
            entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromSeconds(DefaultCacheExpirationInSeconds);
                return client.GetAsync(request, organization);
            });
    }
}