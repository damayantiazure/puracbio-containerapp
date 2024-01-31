using System;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Testing;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests;

public class AzdoRestClientTests
{

    [Fact]
    public async Task DeleteThrowsOnError()
    {
        var request = new AzdoRequest<int>("/delete/some/data");

        using var httpTest = new HttpTest();
        httpTest.RespondWith(status: 500);
        httpTest.RespondWith(status: 500);
        httpTest.RespondWith(status: 500);
        httpTest.RespondWith(status: 500);
        var client = new AzdoRestClient("dummy", "pat");
        await Assert.ThrowsAsync<FlurlHttpException>(async () => await client.DeleteAsync(request));
    }

    [Fact]
    public async Task PostThrowsOnError()
    {
        var request = new AzdoRequest<int, int>("/post/some/data");

        using var httpTest = new HttpTest();
        httpTest.RespondWith(status: 500);
        var client = new AzdoRestClient("dummy", "pat");
        await Assert.ThrowsAsync<FlurlHttpException>(async () => await client.PostAsync(request, 3));
    }

    [Fact]
    public async Task PutThrowsOnError()
    {
        var request = new AzdoRequest<int, int>("/put/some/data");

        using var httpTest = new HttpTest();
        httpTest.RespondWith(status: 500);
        httpTest.RespondWith(status: 500);
        httpTest.RespondWith(status: 500);
        httpTest.RespondWith(status: 500);
        var client = new AzdoRestClient("dummy", "pat");
        await Assert.ThrowsAsync<FlurlHttpException>(async () => await client.PutAsync(request, 3));
    }


    [Fact]
    public async Task GetThrowsOnErrorAfterRetry()
    {
        var request = new AzdoRequest<int>("/get/some/data");

        using var httpTest = new HttpTest();
        httpTest.RespondWith(status: 500);
        httpTest.RespondWith(status: 500);
        httpTest.RespondWith(status: 500);
        httpTest.RespondWith(status: 500);
        var client = new AzdoRestClient("dummy", "pat");
        await Assert.ThrowsAsync<FlurlHttpException>(async () => await client.GetAsync(request));
    }

    [Fact]
    public async Task GetJsonThrowsOnErrorAfterRetry()
    {
        var request = new AzdoRequest<JObject>("/get/some/data");

        using var httpTest = new HttpTest();
        httpTest.RespondWith(status: 500);
        httpTest.RespondWith(status: 500);
        httpTest.RespondWith(status: 500);
        httpTest.RespondWith(status: 500);

        var client = new AzdoRestClient("dummy", "pat");
        await Assert.ThrowsAsync<FlurlHttpException>(async () => await client.GetAsync(request));
    }

    [Fact]
    public async Task GetJsonSucceedsAfterRetry()
    {
        var request = new AzdoRequest<JObject>("/get/some/data");

        using var httpTest = new HttpTest();
        httpTest.RespondWith(status: 502);
        httpTest.RespondWith(status: 502);
        httpTest.RespondWith("{}", 200);
        var client = new AzdoRestClient("dummy", "pat");
        var response = await client.GetAsync(request);
        Assert.NotNull(response);
    }

    [Fact]
    public void EmptyPatShouldNotFailEarlyAndNotThrowWithInvalidHtml() =>
        Assert.Throws<ArgumentNullException>(() => new AzdoRestClient("raboweb-test", null));

    [Fact]
    public void EmptyOrganizationShouldFailEarlyAndNotWithNotFound() =>
        Assert.Throws<ArgumentNullException>(() => new AzdoRestClient(null, "pat"));
}