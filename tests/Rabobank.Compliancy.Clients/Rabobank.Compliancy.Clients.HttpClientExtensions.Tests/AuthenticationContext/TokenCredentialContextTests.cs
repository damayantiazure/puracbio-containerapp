using AutoFixture;
using Azure.Core;
using FluentAssertions;
using Rabobank.Compliancy.Clients.HttpClientExtensions.AuthenticationContext;

namespace Rabobank.Compliancy.Clients.HttpClientExtensions.Tests.AuthenticationContext;
public class TokenCredentialContextTests
{
    private readonly Mock<TokenCredential> _tokenCredentialMock = new();
    private readonly IFixture _fixture = new Fixture();

    private readonly TokenCredentialContext _sut;

    public TokenCredentialContextTests()
    {
        _sut = new TokenCredentialContext(_tokenCredentialMock.Object, new TokenRequestContext());
    }


    [Fact]
    public async Task GetAuthenticationHeaderAsync_ReturnsBearerHeader()
    {
        // Arrange
        var token = _fixture.Create<AccessToken>();
        _tokenCredentialMock.Setup(credential => credential.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>())).ReturnsAsync(token);

        // Act
        var authenticationHeader = await _sut.GetAuthenticationHeaderAsync();

        // Assert
        authenticationHeader.Should().NotBeNull();
        authenticationHeader.Scheme.Should().Be("Bearer");
        authenticationHeader.Parameter.Should().Be(token.Token);
    }
}
