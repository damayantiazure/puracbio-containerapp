using Flurl.Http;
using MemoryCache.Testing.Moq;
using Microsoft.Extensions.Caching.Memory;
using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using static Rabobank.Compliancy.Infra.AzdoClient.Requests.YamlPipeline;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.PipelineResources.Tests.Helpers;

public class YamlHelperTests
{
    private readonly Mock<IAzdoRestClient> _azdoRestClientMock = new();
    private readonly YamlHelper _sut;
    private readonly IFixture _fixture = new Fixture();
    private const string _yamlWithNoStages = "stages:\r\n  - stage: Build\r\n    jobs:\r\n    - job: Release\r\n      steps: ";
    private const string _yamlWithStages = "stages:\r\n  - stage: Build\r\n    jobs:\r\n    - job: Release\r\n      steps:      \r\n      - task: dbb-deploy-prod@1\r\n        inputs:\r\n          serviceEndpoint: 'DummyConnection'\r\n          ucdVersion: '1'\r\n          organizationName: 'DummyOrganization'\r\n          projectId: 'DummyProjectId'\r\n          pipelineId: 'DummyPipelineId'";
    private const string _yamlWithNoTasks = "stages:\r\n  - stage: Build\r\n    jobs:\r\n    - job: Release\r\n      steps:\n        - checkout: self\n          path: src\n ";

    public YamlHelperTests()
    {
        var cachedValue = _fixture.Create<ApplicationGroups>();
        var memoryCache = Create.MockedMemoryCache();
        memoryCache.GetOrCreate("https://dev.azure.com/raboweb/cached/_api/_identity/ReadScopedApplicationGroupsJson?__v=5", entry => cachedValue);

        _sut = new YamlHelper(memoryCache, _azdoRestClientMock.Object);
    }

    [Theory]
    [InlineData(_yamlWithStages, 1)]
    [InlineData(_yamlWithNoStages, 0)]
    [InlineData(_yamlWithNoTasks, 0)]
    public async Task GetPipelineTasksAsync_WithInlineDataOfYamls_ShouldHaveAValidCount(string yaml, int count)
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>().ToString();

        var buildDefinition = _fixture.Build<BuildDefinition>()
            .With(x => x.Yaml, yaml)
            .Without(x => x.YamlUsedInRun).Create();

        // Act
        var actual = await _sut.GetPipelineTasksAsync(organization, projectId, buildDefinition);

        // Assert
        actual.Should().HaveCount(count);
    }

    [Fact]
    public async Task GetPipelineTasksAsync_WithValidYamlUsedInRun_ShouldReturnCollectionWithOnePipelineInputs()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>().ToString();

        var buildDefinition = _fixture.Build<BuildDefinition>()
            .With(x => x.YamlUsedInRun, _yamlWithStages).Create();

        // Act
        var actual = await _sut.GetPipelineTasksAsync(organization, projectId, buildDefinition);

        // Assert
        actual.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPipelineTasksAsync_WithValidYamlFile_ShouldReturnValidInputs()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>().ToString();
        var buildDefinition = _fixture.Build<BuildDefinition>()
            .With(x => x.Yaml, _yamlWithStages)
            .Without(x => x.YamlUsedInRun).Create();

        // Act
        var actual = (await _sut.GetPipelineTasksAsync(organization, projectId, buildDefinition))
            .FirstOrDefault();

        // Assert        
        actual.Inputs.Should().BeEquivalentTo(new Dictionary<string, string>
        {
            { "serviceEndpoint", "DummyConnection" },
            { "ucdVersion", "1"},
            { "organizationName", "DummyOrganization"},
            { "projectId", "DummyProjectId"},
            { "pipelineId", "DummyPipelineId"}
        });
    }

    [Fact]
    public async Task GetPipelineTasksAsync_WithInvalidYamlUsedInRun_ShouldReturnEmptyCollection()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>().ToString();
        var buildDefinition = _fixture.Create<BuildDefinition>();

        // Act
        var actual = await _sut.GetPipelineTasksAsync(organization, projectId, buildDefinition);

        // Assert
        actual.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPipelineTasksAsync_WithNoYaml_ShouldRetrieveYamlAndReturnCollection()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>().ToString();
        var buildDefinition = _fixture.Build<BuildDefinition>()
            .Without(x => x.YamlUsedInRun).Without(x => x.Yaml).Create();

        var yamlPipelineResponse = _fixture.Build<YamlPipelineResponse>()
            .With(x => x.FinalYaml, _yamlWithStages).Create();

        _azdoRestClientMock.Setup(x => x.PostAsync(It.IsAny<IAzdoRequest<YamlPipelineRequest, YamlPipelineResponse>>()
                , It.IsAny<YamlPipelineRequest>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(yamlPipelineResponse);

        // Act
        var actual = (await _sut.GetPipelineTasksAsync(organization, projectId, buildDefinition))
            .FirstOrDefault();

        // Assert
        actual.Should().NotBeNull();
        actual.Inputs.Should().BeEquivalentTo(new Dictionary<string, string>
        {
            { "serviceEndpoint", "DummyConnection" },
            { "ucdVersion", "1"},
            { "organizationName", "DummyOrganization"},
            { "projectId", "DummyProjectId"},
            { "pipelineId", "DummyPipelineId"}
        });
    }

    [Fact]
    public async Task GetPipelineTasksAsync_WithNoFinalYaml_ShouldRetrieveYamlAndReturnEmptyCollection()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>().ToString();
        var buildDefinition = _fixture.Build<BuildDefinition>()
            .Without(x => x.YamlUsedInRun).Without(x => x.Yaml).Create();

        var yamlPipelineResponse = _fixture.Build<YamlPipelineResponse>()
            .Without(x => x.FinalYaml).Create();

        _azdoRestClientMock.Setup(x => x.PostAsync(It.IsAny<IAzdoRequest<YamlPipelineRequest, YamlPipelineResponse>>()
                , It.IsAny<YamlPipelineRequest>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(yamlPipelineResponse);

        // Act
        var actual = (await _sut.GetPipelineTasksAsync(organization, projectId, buildDefinition))
            .FirstOrDefault();

        // Assert
        actual.Should().BeNull();
    }

    [Theory]
    [InlineData((int)HttpStatusCode.BadRequest)]
    [InlineData((int)HttpStatusCode.NotFound)]
    [InlineData((int)HttpStatusCode.InternalServerError)]
    public async Task GetPipelineTasksAsync_WithFlurlHttpException_ShouldReturnEmptyCollection(int httpStatusCode)
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>().ToString();
        var buildDefinition = _fixture.Build<BuildDefinition>()
            .Without(x => x.YamlUsedInRun).Without(x => x.Yaml).Create();

        var exception = new FlurlHttpException(new HttpCall
        {
            Response = new HttpResponseMessage
            {
                StatusCode = (HttpStatusCode)httpStatusCode
            }
        }, "", new Exception());

        _azdoRestClientMock.Setup(x => x.PostAsync(It.IsAny<IAzdoRequest<YamlPipelineRequest, YamlPipelineResponse>>()
                , It.IsAny<YamlPipelineRequest>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ThrowsAsync(exception);

        // Act
        var actual = (await _sut.GetPipelineTasksAsync(organization, projectId, buildDefinition))
            .FirstOrDefault();

        // Assert
        actual.Should().BeNull();
    }

    [Fact]
    public async Task GetPipelineTasksAsync_WithFlurlHttpException_ShouldThrowFlurlHttpException()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>().ToString();
        var buildDefinition = _fixture.Build<BuildDefinition>()
            .Without(x => x.YamlUsedInRun).Without(x => x.Yaml).Create();

        var exception = new FlurlHttpException(new HttpCall
        {
            Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized
            }
        }, "", new Exception());

        _azdoRestClientMock.Setup(x => x.PostAsync(It.IsAny<IAzdoRequest<YamlPipelineRequest, YamlPipelineResponse>>()
                , It.IsAny<YamlPipelineRequest>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ThrowsAsync(exception);

        // Act
        Func<Task<IEnumerable<Model.PipelineTaskInputs>>> actual = () => _sut.GetPipelineTasksAsync(organization, projectId, buildDefinition);

        // Assert
        await actual.Should().ThrowAsync<FlurlHttpException>();
    }
}