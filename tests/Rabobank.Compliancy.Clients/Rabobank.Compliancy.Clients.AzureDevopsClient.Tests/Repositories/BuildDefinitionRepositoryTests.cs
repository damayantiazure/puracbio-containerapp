using AutoFixture.AutoMoq;
using Microsoft.TeamFoundation.Build.WebApi;
using Newtonsoft.Json;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;
using Rabobank.Compliancy.Clients.HttpClientExtensions;
using Rabobank.Compliancy.Domain.Enums;
using System.Net.Http.Headers;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Build.Models;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.Repositories;

public class BuildDefinitionRepositoryTests
{
    private readonly IFixture _fixture = new Fixture();

    private readonly Mock<IDevHttpClientCallHandler> _httpClientCallHandlerMock = new();

    private readonly AzureDevopsClient.Repositories.BuildRepository _sut;

    /// The <see cref="Timeline"/> class is marked as sealed and an instance cannot be created by autofixture.
    private const string TimelineJson = @"{'records':[{'previousAttempts':[],'id':'4440cc73-a60e-5d41-0d45-fbcca41e4038','parentId':'96ac2280-8cb4-5df5-99de-dd2da759617d','type':'Checkpoint','name':'Checkpoint','startTime':'2023-09-13T15:35:36.42Z','finishTime':'2023-09-13T15:35:36.42Z','currentOperation':null,'percentComplete':null,'state':'completed','result':'succeeded','resultCode':null,'changeId':2,'lastModified':'0001-01-01T00:00:00','workerName':null,'details':null,'errorCount':0,'warningCount':0,'url':null,'log':null,'task':null,'attempt':1,'identifier':'Checkpoint'}],'lastChangedBy':'00000002-0000-8888-8000-000000000000','lastChangedOn':'2023-09-13T15:35:50.703Z','id':'babb70ca-0883-4206-921b-65a670f00694','changeId':15,'url':'https://dev.azure.com/raboweb-test/555efb82-c8b1-4866-a301-9e6dab68d734/_apis/build/builds/488192/Timeline/babb70ca-0883-4206-921b-65a670f00694'}";

    public BuildDefinitionRepositoryTests()
    {
        _sut = new AzureDevopsClient.Repositories.BuildRepository(_httpClientCallHandlerMock.Object);

        _fixture.Customize<Build>(x => x
            .Without(x => x.RequestedFor)
            .Without(x => x.RequestedBy)
            .Without(x => x.LastChangedBy)
            .Without(x => x.DeletedBy)
        );

        _fixture.Customize<Change>(x => x
            .Without(x => x.Author)
        );

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _fixture.Customize(new AutoMoqCustomization());
    }

    [Fact]
    public async Task GetBuildDefinitionByIdAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var buildDefinition = new BuildDefinition();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<BuildDefinition>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(buildDefinition);

        // Act
        var actual = await _sut.GetBuildDefinitionByIdAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<int>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(buildDefinition);
    }

    [Theory]
    [InlineData("1", PipelineProcessType.DesignerBuild)]
    [InlineData("2", PipelineProcessType.Yaml)]
    [InlineData("0", PipelineProcessType.DesignerRelease)]
    public async Task GetBuildDefinitionByIdAsync_WithPipelineProcessType_CreatesCorrectRequestUri(string expectedAzdoInteger, PipelineProcessType type)
    {
        // Arrange
        var buildDefinition = new BuildDefinition { Id = _fixture.Create<int>() };

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<BuildDefinition>(It.Is<Uri>(u => u.ToString().Contains($"processType={expectedAzdoInteger}")), It.IsAny<AuthenticationHeaderValue>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(buildDefinition)
            .Verifiable();

        // Act
        var actual = await _sut.GetBuildDefinitionByIdAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<int>(), type, It.IsAny<CancellationToken>());

        // Assert
        actual.Should().NotBeNull();
        actual.Should().BeEquivalentTo(buildDefinition);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetBuildDefinitionsByProjectAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var buildDefinition1 = new BuildDefinition { Id = _fixture.Create<int>() };
        var buildDefinition2 = new BuildDefinition { Id = _fixture.Create<int>() };
        var buildDefinitions = new ResponseCollection<BuildDefinition> { Count = 2, Value = new[] { buildDefinition1, buildDefinition2 } };

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ResponseCollection<BuildDefinition>>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(buildDefinitions);

        // Act
        var actual = await _sut.GetBuildDefinitionsByProjectAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<bool>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().NotBeNull();
        actual.Should().BeEquivalentTo(buildDefinitions.Value);
    }

    [Theory]
    [InlineData("1", PipelineProcessType.DesignerBuild)]
    [InlineData("2", PipelineProcessType.Yaml)]
    [InlineData("0", PipelineProcessType.DesignerRelease)]
    public async Task GetBuildDefinitionsByProjectAsync_WithPipelineProcessType_CreatesCorrectRequestUri(string expectedAzdoInteger, PipelineProcessType type)
    {
        // Arrange
        var buildDefinition1 = new BuildDefinition { Id = _fixture.Create<int>() };
        var buildDefinition2 = new BuildDefinition { Id = _fixture.Create<int>() };
        var buildDefinitions = new ResponseCollection<BuildDefinition> { Count = 2, Value = new[] { buildDefinition1, buildDefinition2 } };

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ResponseCollection<BuildDefinition>>(It.Is<Uri>(u => u.ToString().Contains($"processType={expectedAzdoInteger}")), It.IsAny<AuthenticationHeaderValue>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(buildDefinitions)
            .Verifiable();

        // Act
        var actual = await _sut.GetBuildDefinitionsByProjectAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), type, _fixture.Create<bool>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().NotBeNull();
        actual.Should().BeEquivalentTo(buildDefinitions.Value);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetPipelineClassicBuildYaml_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var pipelineClassicBuildYaml = new PipelineClassicBuildYaml { Yaml = _fixture.Create<string>() };

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<PipelineClassicBuildYaml>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pipelineClassicBuildYaml);

        // Act
        var actual = await _sut.GetPipelineClassicBuildYaml(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<int>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().NotBeNull();
        actual.Should().Be(pipelineClassicBuildYaml.Yaml);
    }

    [Fact]
    public async Task GetBuildAsync_WithExistingBuild_ShouldReturnTheBuildInformation()
    {
        // Arrange
        var responseCollection = new ResponseCollection<Build> { Value = _fixture.CreateMany<Build>(1) };

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ResponseCollection<Build>>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseCollection);

        // Act
        var actual = (await _sut.GetBuildAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<int>(), It.IsAny<CancellationToken>()))!;

        // Assert
        actual.Should().NotBeNull();
        actual.Should().BeEquivalentTo(responseCollection.Value);
    }

    [Fact]
    public async Task GetBuildChangesAsync_WithExistingBuild_ShouldReturnACollectionOfBuildChanges()
    {
        // Arrange
        var responseCollection = new ResponseCollection<Change> { Value = _fixture.CreateMany<Change>(1) };

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ResponseCollection<Change>>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseCollection);

        // Act
        var actual = (await _sut.GetBuildChangesAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<int>(), It.IsAny<CancellationToken>()))!;

        // Assert
        actual.Should().NotBeNull();
        actual.Should().BeEquivalentTo(responseCollection.Value);
    }

    [Fact]
    public async Task GetBuildTimelineAsync_WithExistingBuild_ShouldReturnBuildTimelineInformation()
    {
        // Arrange
        var timeline = JsonConvert.DeserializeObject<Timeline>(TimelineJson);
        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<Timeline>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(timeline);

        // Act
        var actual = (await _sut.GetBuildTimelineAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<int>(), It.IsAny<CancellationToken>()))!;

        // Assert
        actual.Should().NotBeNull();
        actual.Should().BeEquivalentTo(timeline);
    }

    [Fact]
    public async Task GetProjectRetentionAsync_WithExistingBuild_ShouldReturnACollectionOfBuildTags()
    {
        // Arrange
        var projectRetentionSettings = _fixture.Create<ProjectRetentionSetting>();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ProjectRetentionSetting>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectRetentionSettings);

        // Act
        var actual = (await _sut.GetProjectRetentionAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), It.IsAny<CancellationToken>()))!;

        // Assert
        actual.Should().NotBeNull();
        actual.Should().BeEquivalentTo(projectRetentionSettings);
    }

    [Fact]
    public async Task GetBuildTagsAsync_WithExistingBuild_ShouldReturnACollectionOfBuildTags()
    {
        // Arrange
        var responseCollection = new ResponseCollection<string> { Value = _fixture.CreateMany<string>(1) };

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ResponseCollection<string>>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseCollection);

        // Act
        var actual = (await _sut.GetBuildTagsAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<int>(), It.IsAny<CancellationToken>()))!;

        // Assert
        actual.Should().NotBeNull();
        actual.Should().BeEquivalentTo(responseCollection.Value);
    }

    [Fact]
    public async Task SetProjectRetentionAsync_WithChangedRetentionSetting_ShouldUpdateTheProjectRetentionSettings()
    {
        // Arrange
        var projectRetentionSettings = _fixture.Create<ProjectRetentionSetting?>();

        _httpClientCallHandlerMock.Setup(x => x.HandlePatchCallAsync<ProjectRetentionSetting?, UpdateProjectRetentionSettingModel>(It.IsAny<Uri>(),
                It.IsAny<UpdateProjectRetentionSettingModel>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectRetentionSettings);

        // Act
        var actual = (await _sut.SetProjectRetentionAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<UpdateProjectRetentionSettingModel>(),
            It.IsAny<CancellationToken>()))!;

        // Assert
        actual.Should().NotBeNull();
        actual.Should().BeEquivalentTo(projectRetentionSettings);
    }

    [Fact]
    public async Task AddTagsToBuildAsync_WithNewBuildTags_ShouldAddNewBuildTags()
    {
        // Arrange
        var tags = _fixture.CreateMany<string>().ToList();
        var responseCollection = new ResponseCollection<string> { Value = tags };

        _httpClientCallHandlerMock.Setup(x => x.HandlePatchCallAsync<ResponseCollection<string>?, UpdateTagParameters>(It.IsAny<Uri>(),
                It.IsAny<UpdateTagParameters>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseCollection);

        // Act
        var actual = (await _sut.AddTagsToBuildAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<int>(), tags,
          It.IsAny<CancellationToken>()))!;

        // Assert
        actual.Should().NotBeNull();
        actual.Should().BeEquivalentTo(tags);
    }

    [Fact]
    public async Task RemoveTagsFromBuildAsync_WithBuildTags_ShouldRemoveBuildTags()
    {
        // Arrange
        var tagToBeRemoved = _fixture.CreateMany<string>(1).ToList();
        var tags = _fixture.CreateMany<string>();
        var responseCollection = new ResponseCollection<string> { Value = tags };

        _httpClientCallHandlerMock.Setup(x => x.HandlePatchCallAsync<ResponseCollection<string>?, UpdateTagParameters>(It.IsAny<Uri>(),
                It.IsAny<UpdateTagParameters>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseCollection);

        // Act
        var actual = (await _sut.RemoveTagsFromBuildAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<int>(), tagToBeRemoved,
          It.IsAny<CancellationToken>()))!;

        // Assert
        actual.Should().NotBeNull();
        actual.Should().NotContain(tagToBeRemoved);
    }
}