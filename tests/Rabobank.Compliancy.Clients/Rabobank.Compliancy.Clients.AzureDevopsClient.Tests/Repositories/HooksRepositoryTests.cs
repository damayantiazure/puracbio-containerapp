using Microsoft.VisualStudio.Services.ServiceHooks.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Hooks.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.Repositories;

public class HooksRepositoryTests
{
    private readonly IFixture _fixture = new Fixture();

    private readonly Mock<IDevHttpClientCallHandler> _httpClientCallHandlerMock = new();
    private readonly Mock<IVsrmHttpClientCallHandler> _httpVsrmClientCallHandlerMock = new();

    private readonly HooksRepository _sut;

    public HooksRepositoryTests()
    {
        _sut = new HooksRepository(_httpClientCallHandlerMock.Object, _httpVsrmClientCallHandlerMock.Object);
    }

    [Fact]
    public async Task GetSubscriptionsAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var subscription1 = new Subscription();
        var subscription2 = new Subscription();
        var response = new ResponseCollection<Subscription> { Count = 2, Value = new[] { subscription1, subscription2 } };

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ResponseCollection<Subscription>>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var actual = await _sut.GetSubscriptionsAsync(_fixture.Create<string>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().NotBeNull();
        actual.Should().Contain(x => x.Id.Equals(subscription1.Id));
        actual.Should().Contain(x => x.Id.Equals(subscription2.Id));
    }

    [Fact]
    public async Task GetHookNotificationsAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var notification1 = _fixture.Create<Notification>();
        var notification2 = _fixture.Create<Notification>();
        var response = new ResponseCollection<Notification> { Count = 2, Value = new[] { notification1, notification2 } };

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ResponseCollection<Notification>>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var actual = await _sut.GetHookNotificationsAsync(_fixture.Create<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().NotBeNull();
        actual.Should().Contain(x => x.Id.Equals(notification1.Id));
        actual.Should().Contain(x => x.Id.Equals(notification2.Id));
    }

    [Fact]
    public async Task GetHookNotificationAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var response = _fixture.Create<Notification>();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<Notification>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var actual = await _sut.GetHookNotificationAsync(_fixture.Create<string>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task GetSubscriptionAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var response = new Subscription();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<Subscription>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var actual = await _sut.GetSubscriptionAsync(_fixture.Create<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task AddHookSubscriptionAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var response = new Subscription();
        _httpClientCallHandlerMock.Setup(x => x.HandlePostCallAsync<Subscription, Subscription>(It.IsAny<Uri>(), It.IsAny<Subscription>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var actual = await _sut.AddHookSubscriptionAsync(_fixture.Create<string>(), It.IsAny<Subscription>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task AddReleaseManagementSubscriptionAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var response = new SubscriptionsQuery();
        _httpVsrmClientCallHandlerMock.Setup(x => x.HandlePostCallAsync<SubscriptionsQuery, ReleaseManagementSubscriptionBodyContent>(It.IsAny<Uri>(), It.IsAny<ReleaseManagementSubscriptionBodyContent>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var actual = await _sut.AddReleaseManagementSubscriptionAsync(_fixture.Create<string>(), _fixture.Create<ReleaseManagementSubscriptionBodyContent>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(response);
    }
}