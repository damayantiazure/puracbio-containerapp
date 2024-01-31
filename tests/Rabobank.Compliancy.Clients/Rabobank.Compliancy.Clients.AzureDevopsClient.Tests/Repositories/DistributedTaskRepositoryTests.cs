using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.DistributedTask.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.Repositories;

public class DistributedTaskRepositoryTests
{
    private readonly IFixture _fixture = new Fixture();

    private readonly Mock<IDevHttpClientCallHandler> _httpClientCallHandlerMock = new();

    private readonly DistributedTaskRepository _sut;

    public DistributedTaskRepositoryTests()
    {
        _sut = new DistributedTaskRepository(_httpClientCallHandlerMock.Object);
    }

    [Fact]
    public async Task AddTaskStartedEventAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var response = "";
        _httpClientCallHandlerMock.Setup(x => x.HandlePostCallAsync<string, AddTaskEventBodyContent>(It.IsAny<Uri>(), It.IsAny<AddTaskEventBodyContent>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var actual = await _sut.AddTaskStartedEventAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<string>(),
            _fixture.Create<Guid>(), _fixture.Create<AddTaskEventBodyContent>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task AddTaskCompletedEventAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var response = "";
        _httpClientCallHandlerMock.Setup(x => x.HandlePostCallAsync<string, AddTaskEventBodyContent>(It.IsAny<Uri>(), It.IsAny<AddTaskEventBodyContent>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var actual = await _sut.AddTaskCompletedEventAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<string>(),
            _fixture.Create<Guid>(), _fixture.Create<AddTaskEventBodyContent>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task CreateTaskLogAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var response = _fixture.Create<TaskLog>();
        _httpClientCallHandlerMock.Setup(x => x.HandlePostCallAsync<TaskLog, TaskLog>(It.IsAny<Uri>(), It.IsAny<TaskLog>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var actual = await _sut.CreateTaskLogAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<string>(),
            _fixture.Create<Guid>(), _fixture.Create<TaskLog>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task AppendToTaskLogAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var response = _fixture.Create<TaskLog>();
        _httpClientCallHandlerMock.Setup(x => x.HandlePostCallAsync<TaskLog, byte[]>(It.IsAny<Uri>(), It.IsAny<byte[]>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var actual = await _sut.AppendToTaskLogAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<string>(),
            _fixture.Create<Guid>(), _fixture.Create<int>(), _fixture.Create<string>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(response);
    }
}