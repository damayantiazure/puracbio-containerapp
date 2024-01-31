using Rabobank.Compliancy.Clients.AzureDevopsClient.DelegatingHandlers;
using Rabobank.Compliancy.Clients.HttpClientExtensions.AuthenticationContext;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.DelegatingHandlers;

public class IdentityProviderTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IAuthenticationHeaderContext> _authenticationHeaderContextMock = new();

    [Fact]
    public void GetIdentityContext_WithIdentityContextPresent_ReturnsCorrectIdentityContext()
    {
        // Arrange
        var uniqueIdentifier = _fixture.Create<string>();
        _authenticationHeaderContextMock.Setup(context => context.Identifier).Returns(uniqueIdentifier);
        var sut = new IdentityProvider(new[] { _authenticationHeaderContextMock.Object });

        // Act
        var actual = sut.GetIdentityContext(uniqueIdentifier);

        // Assert
        actual.Should().NotBeNull();
        actual.Identifier.Should().Be(uniqueIdentifier);
    }
}