using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.Repositories;

public class TaskGroupRepositoryTests
{
    private readonly IFixture _fixture = new Fixture();

    private readonly Mock<IDevHttpClientCallHandler> _httpClientCallHandlerMock
        = new();

    private readonly TaskGroupRepository _sut;

    public TaskGroupRepositoryTests()
    {
        _sut = new TaskGroupRepository(_httpClientCallHandlerMock.Object);
    }

    [Fact]
    public async Task GetTaskGroupsAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var taskGroup1 = new TaskGroup { Id = _fixture.Create<Guid>() };
        var taskGroup2 = new TaskGroup { Id = _fixture.Create<Guid>() };
        var taskGroups = new ResponseCollection<TaskGroup> { Count = 2, Value = new[] { taskGroup1, taskGroup2 } };

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ResponseCollection<TaskGroup>>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskGroups).Verifiable();

        // Act
        var actual = await _sut.GetTaskGroupsAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().NotBeNull();
        actual.Should().Contain(x => x.Id.Equals(taskGroup1.Id));
        actual.Should().Contain(x => x.Id.Equals(taskGroup2.Id));
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetTaskGroupByIdAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var taskGroup = new TaskGroup { Id = _fixture.Create<Guid>() };
        var response = new ResponseCollection<TaskGroup> { Count = 1, Value = new[] { taskGroup } };
        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ResponseCollection<TaskGroup>>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var actual = await _sut.GetTaskGroupByIdAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().NotBeNull();
        actual.Should().Contain(x => x.Id.Equals(taskGroup.Id));
    }
}