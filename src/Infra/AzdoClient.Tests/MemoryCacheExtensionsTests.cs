using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using MemoryCache.Testing.Moq;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Rabobank.Compliancy.Infra.AzdoClient.Extensions;
using Xunit;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests;

public class MemoryCacheExtensionsTests
{
    [Fact]
    public async Task CanCacheRestClientRequests()
    {
        var fixture = new Fixture();
        fixture.Customize(new AutoNSubstituteCustomization());

        var cachedValue = fixture.Create<Response.ApplicationGroups>();
        var uncachedValue = fixture.Create<Response.ApplicationGroups>();

        var cache = Create.MockedMemoryCache();
        cache.GetOrCreate("https://dev.azure.com/raboweb/cached/_api/_identity/ReadScopedApplicationGroupsJson?__v=5", entry => cachedValue);

        var uncachedRequest = Requests.ApplicationGroup.ApplicationGroups("uncached");
        var cachedRequest = Requests.ApplicationGroup.ApplicationGroups("cached");

        var client = new Mock<IAzdoRestClient>();
        client.Setup(x => x.GetAsync(uncachedRequest, "raboweb")).ReturnsAsync(uncachedValue);

        var uncachedResponse = await cache.GetOrCreateAsync(client.Object, uncachedRequest, "raboweb");
        var cachedResponse = await cache.GetOrCreateAsync(client.Object, cachedRequest, "raboweb");

        Assert.Same(uncachedValue, uncachedResponse);
        Assert.Same(cachedValue, cachedResponse);
    }
}