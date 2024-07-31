using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests.Integration.Requests;

[Trait("Category", "integration")]
public class SecurityNamespaceTests : IClassFixture<TestConfig>
{
    private readonly IAzdoRestClient _client;

    public SecurityNamespaceTests(TestConfig config)
    {
        _client = new AzdoRestClient(config.Organization, config.Token);
    }

    [Fact]
    public async Task QueryNamespaces()
    {
        var target = (await _client.GetAsync(SecurityNamespace.SecurityNamespaces())).ToList();
        target.ShouldNotBeEmpty();

        var first = target.First();
        first.Actions.ShouldNotBeEmpty();

        var action = first.Actions.First();
        action.Name.ShouldNotBeEmpty();
        action.DisplayName.ShouldNotBeEmpty();
        action.Bit.ShouldNotBe(0);
    }
}