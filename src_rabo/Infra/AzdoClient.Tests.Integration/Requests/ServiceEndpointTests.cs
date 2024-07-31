using AutoFixture;
using Rabobank.Compliancy.Infra.AzdoClient.Extensions;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests.Integration.Requests;

[Trait("category", "integration")]
public class ServiceEndpointTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;

    public ServiceEndpointTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(config.Organization, config.Token);
    }

    [Fact]
    public async Task QueryServiceConnections()
    {
        var endpoints = await _client.GetAsync(ServiceEndpoint.Endpoints(_config.ProjectName));
        endpoints.ShouldNotBeEmpty();

        var endpoint = endpoints.First();
        endpoint.Name.ShouldNotBeNullOrEmpty();
        endpoint.Id.ShouldNotBe(Guid.Empty);
        endpoint.Type.ShouldNotBeNullOrEmpty();
        endpoint.Url.ToString().ShouldNotBeNullOrEmpty();
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task CreateAndDeleteEndpoint()
    {
        var fixture = new Fixture();
        var name = fixture.Create<string>().Substring(0, 10);

        var endpoint = await _client.PostAsync(ServiceEndpoint.Endpoint(_config.ProjectName), new Response.ServiceEndpoint
        {
            Name = name,
            Type = "generic",
            Url = new Uri("https://localhost"),
            Authorization = Response.ServiceEndpointAuthorization.UserNamePassword("test", "abc")
        });

        endpoint.Name.ShouldBe(name);
        endpoint.Type.ShouldBe("generic");
        endpoint.Url.ToString().ShouldBe("https://localhost/");

        await _client.DeleteAsync(ServiceEndpoint.Endpoint(_config.ProjectName, endpoint.Id));
        await Task.Delay(TimeSpan.FromSeconds(10));

        var deletedEndpoint = await _client.GetAsync(ServiceEndpoint.Endpoint(_config.ProjectName, endpoint.Id));
        deletedEndpoint.ShouldBeNull();
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task RestRequestResultAsJsonObject()
    {
        var endpoints = await _client.GetAsync(ServiceEndpoint.Endpoints(_config.ProjectName).Request.AsJson());
        endpoints.SelectToken("value[?(@.data.subscriptionId == '45cfa52a-a2aa-4a18-8d3d-29896327b51d')]").ShouldNotBeNull();
    }
}