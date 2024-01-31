using Microsoft.TeamFoundation.Dashboards.WebApi;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Infrastructure.Tests.Security;
public class SecurityContextTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task ResolveUserFromToken_TokenPresent_CallsAuthorizationServiceWithCorrectParams()
    {
        // Arrange        
        var organization = _fixture.Create<string>();
        var token = _fixture.Create<string>();

        var authorizationServiceMock = new Mock<IAuthorizationService>();
        authorizationServiceMock
            .Setup(m => m.GetCurrentUserAsync(It.IsAny<string>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = Guid.NewGuid() });

        var securityContext = new Infrastructure.Security.SecurityContext(authorizationServiceMock.Object);

        // Act
        await securityContext.ResolveUserFromToken($"Bearer {token}", organization);

        // Assert
        authorizationServiceMock.Verify(m => m.GetCurrentUserAsync(organization,
            It.Is<AuthenticationHeaderValue>(h => h.Parameter == token), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResolveUserFromToken_TokenPresent_SetsUserOnContext()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var authorizationServiceMock = new Mock<IAuthorizationService>();
        var securityContext = new Infrastructure.Security.SecurityContext(authorizationServiceMock.Object);
        authorizationServiceMock
            .Setup(m => m.GetCurrentUserAsync(It.IsAny<string>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

        // Act
        await securityContext.ResolveUserFromToken($"Bearer secret", "raboweb-test");

        // Assert
        securityContext.UserId.Should().Be(userId);
    }
}