using AutoFixture;
using FluentAssertions;
using Rabobank.Compliancy.Clients.HttpClientExtensions.AuthenticationContext;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.DelegatingHandlers;

public class PersonalAccessTokenContextTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public async Task GetAuthenticationHeaderAsync_CredentialSet_ReturnsHeader()
    {
        // Arrange
        var identifier = _fixture.Create<string>();
        var context = new PersonalAccessTokenContext(identifier);

        // Act
        var header = await context.GetAuthenticationHeaderAsync();

        // Assert
        header.Scheme.ToLowerInvariant().Should().Be("basic");
        header.Parameter.Should().NotBeNull();
    }
}