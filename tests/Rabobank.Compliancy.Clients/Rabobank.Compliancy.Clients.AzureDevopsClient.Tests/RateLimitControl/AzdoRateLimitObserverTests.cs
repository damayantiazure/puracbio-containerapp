using Microsoft.Extensions.Logging;
using Rabobank.Compliancy.Clients.AzureDevopsClient.RateLimitControl;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.RateLimitControl;

public class AzdoRateLimitObserverTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void AzdoRateLimitObserver_EmptyIdentityList_ThrowsException()
    {
        // Arrange & Act
        Action observerCreation = () => new AzdoRateLimitObserver(new string[] { }, null!);

        // Assert
        observerCreation.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AzdoRateLimitObserver_NullIdentityList_ThrowsException()
    {
        // Arrange & Act
        Action observerCreation = () => new AzdoRateLimitObserver(null!, null!);

        // Assert
        observerCreation.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SetRateLimitDelayForIdentity_TwoIdentitiesSet_ReturnsAvailableIdentity()
    {
        // Arrang
        var identity = _fixture.CreateMany<string>(2).ToArray();

        DateTime rateLimitUntil = DateTime.Now.AddMinutes(5);
        var loggerMock = new Mock<ILogger<AzdoRateLimitObserver>>();
        var observer = new AzdoRateLimitObserver(identity, loggerMock.Object);

        // Act
        observer.SetRateLimitDelayForIdentity(identity[0], rateLimitUntil);

        // Assert
        var availableIdentity = observer.GetAvailableIdentity();
        availableIdentity.Should().Be(identity[1]);
    }

    [Fact]
    public void GetAvailableIdentity_WithNoRateLimit_ShouldNotLogWarningAndReturnAvailableIdentities()
    {
        // Arrange
        var identity = _fixture.Create<string>();
        var loggerMock = new Mock<ILogger<AzdoRateLimitObserver>>();
        var observer = new AzdoRateLimitObserver(new string[] { identity }, loggerMock.Object);

        // Act
        var availableIdentity = observer.GetAvailableIdentity();

        // Assert
        availableIdentity.Should().Be(identity);
        loggerMock.Verify(m => m.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }

    [Fact]
    public void GetAvailableIdentity_WhenRatelimitIsExceeded_ShouldOnlyReturnAvailableIdentities()
    {
        // Arrange
        var identity = _fixture.Create<string>();
        DateTime rateLimitUntil = DateTime.Now.AddMinutes(-5);
        var loggerMock = new Mock<ILogger<AzdoRateLimitObserver>>();
        var observer = new AzdoRateLimitObserver(new string[] { identity }, loggerMock.Object);
        observer.SetRateLimitDelayForIdentity(identity, rateLimitUntil);
        observer.RemoveRateLimitDelayForIdentity(identity);

        // Act
        var availableIdentity = observer.GetAvailableIdentity();

        // Assert
        availableIdentity.Should().Be(identity);
        loggerMock.Verify(m => m.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }

    [Fact]
    public void GetAvailableIdentity_WithNoAvailableIdentities_ShouldLogWarningAndRetrieveTheFirstIdentity()
    {
        // Arrange
        var identity = _fixture.Create<string>();
        DateTime rateLimitUntil = DateTime.Now.AddMinutes(5);
        var loggerMock = new Mock<ILogger<AzdoRateLimitObserver>>();
        var observer = new AzdoRateLimitObserver(new string[] { identity }, loggerMock.Object);
        observer.SetRateLimitDelayForIdentity(identity, rateLimitUntil);
        observer.RemoveRateLimitDelayForIdentity(identity);

        // Act
        var actual = observer.GetAvailableIdentity();

        // Assert
        actual.Should().Be(identity);
        loggerMock.Verify(m => m.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void RemoveRateLimitDelayForIdentity_ThatHasActiveRateLimit_ShouldNotLogInformationAndRemoveTheRateLimit()
    {
        // Arrange
        var identity = _fixture.Create<string>();
        DateTime rateLimitUntil = DateTime.Now.AddMinutes(5);
        var loggerMock = new Mock<ILogger<AzdoRateLimitObserver>>();
        var observer = new AzdoRateLimitObserver(new string[] { identity }, loggerMock.Object);
        observer.SetRateLimitDelayForIdentity(identity, rateLimitUntil);

        // Act
        observer.RemoveRateLimitDelayForIdentity(identity);

        // Assert
        loggerMock.Verify(m => m.Log(LogLevel.Information, It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!
                .Contains("Ratelimit is removed for identity at index 0")),
            It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }

    [Fact]
    public void RemoveRateLimitDelayForIdentity_WhenRateLimitExceeded_ShouldLogInformationAndRemoveTheRateLimit()
    {
        // Arrange
        var identity = _fixture.Create<string>();
        DateTime rateLimitUntil = DateTime.Now.AddMinutes(-5);
        var loggerMock = new Mock<ILogger<AzdoRateLimitObserver>>();
        var observer = new AzdoRateLimitObserver(new string[] { identity }, loggerMock.Object);
        observer.SetRateLimitDelayForIdentity(identity, rateLimitUntil);

        // Act
        observer.RemoveRateLimitDelayForIdentity(identity);

        // Assert
        loggerMock.Verify(m => m.Log(LogLevel.Information, It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!
                .Contains("Ratelimit is removed for identity at index 0")),
            It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}