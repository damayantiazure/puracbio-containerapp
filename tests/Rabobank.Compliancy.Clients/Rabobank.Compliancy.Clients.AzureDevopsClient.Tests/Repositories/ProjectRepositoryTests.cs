using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Operations;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.Repositories;

public class ProjectRepositoryTests
{
    private readonly IFixture _fixture = new Fixture();

    private readonly Mock<IDevHttpClientCallHandler> _httpClientCallHandlerMock = new();
    private readonly ProjectRepository _sut;

    public ProjectRepositoryTests()
    {
        _sut = new ProjectRepository(_httpClientCallHandlerMock.Object);
    }

    [Fact]
    public async Task CreateProjectAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var operation = _fixture.Create<Operation?>();

        _httpClientCallHandlerMock.Setup(x => x.HandlePostCallAsync<Operation?, TeamProject>(It.IsAny<Uri>(), It.IsAny<TeamProject>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(operation).Verifiable();

        // Act
        var actual = await _sut.CreateProjectAsync(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(operation);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetProjectByNameAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var teamProject = _fixture.Create<TeamProject?>();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<TeamProject?>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(teamProject).Verifiable();

        // Act
        var actual = await _sut.GetProjectByNameAsync(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<bool>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(teamProject);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetProjectByIdAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var teamProject = _fixture.Create<TeamProject?>();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<TeamProject?>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(teamProject).Verifiable();

        // Act
        var actual = await _sut.GetProjectByIdAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<bool>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(teamProject);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task DeleteProjectAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var operation = _fixture.Create<Operation?>();

        _httpClientCallHandlerMock.Setup(x => x.HandleDeleteCallAsync<Operation?>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(operation).Verifiable();

        // Act
        var actual = await _sut.DeleteProjectAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(operation);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetProjectsAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var teamProject1 = _fixture.Create<TeamProject>();
        var teamProject2 = _fixture.Create<TeamProject>();
        var teamProjects = new ResponseCollection<TeamProject> { Count = 2, Value = new[] { teamProject1, teamProject2 } };

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ResponseCollection<TeamProject>>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(teamProjects).Verifiable();

        // Act
        var actual = await _sut.GetProjectsAsync(_fixture.Create<string>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().NotBeNull();
        actual.Should().Contain(x => x.Id.Equals(teamProject1.Id));
        actual.Should().Contain(x => x.Id.Equals(teamProject2.Id));
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetProjectPropertiessAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var projectProperty1 = _fixture.Create<ProjectProperty>();
        var projectProperty2 = _fixture.Create<ProjectProperty>();
        var projectProperties = new ResponseCollection<ProjectProperty> { Count = 2, Value = new[] { projectProperty1, projectProperty2 } };

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ResponseCollection<ProjectProperty>>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectProperties).Verifiable();

        // Act
        var actual = await _sut.GetProjectPropertiesAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().NotBeNull();
        actual.Should().Contain(x => x.Name.Equals(projectProperty1.Name));
        actual.Should().Contain(x => x.Name.Equals(projectProperty2.Name));
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetProjectInfoAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var projectInfo = _fixture.Create<ProjectInfo?>();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ProjectInfo?>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectInfo).Verifiable();

        // Act
        var actual = await _sut.GetProjectInfoAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(projectInfo);
        _httpClientCallHandlerMock.Verify();
    }
}