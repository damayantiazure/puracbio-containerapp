using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.WorkItemTracking.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.Repositories;

public class WorkItemTrackingRepositoryTests
{
    private readonly IFixture _fixture = new Fixture();

    private readonly Mock<IDevHttpClientCallHandler> _httpClientCallHandlerMock = new();

    private readonly WorkItemTrackingRepository _sut;

    public WorkItemTrackingRepositoryTests()
    {
        _sut = new WorkItemTrackingRepository(_httpClientCallHandlerMock.Object);
    }

    [Fact]
    public async Task GetQueryByWiqlAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var response = _fixture.Create<WorkItemQueryResult>();
        _httpClientCallHandlerMock.Setup(x => x.HandlePostCallAsync<WorkItemQueryResult, object>(It.IsAny<Uri>(), It.IsAny<object>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var actual = await _sut.GetQueryByWiqlAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<string>(), _fixture.Create<GetQueryBodyContent>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(response);
    }
}