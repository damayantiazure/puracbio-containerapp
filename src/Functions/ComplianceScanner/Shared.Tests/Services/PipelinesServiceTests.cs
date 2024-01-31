using AutoFixture;
using AutoFixture.AutoMoq;
using Flurl.Http;
using Moq;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Model;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using Shouldly;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Xunit;
using static Rabobank.Compliancy.Infra.AzdoClient.Requests.YamlPipeline;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Tests.Services;

public class PipelinesServiceTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly Mock<IAzdoRestClient> _azdoClient = new Mock<IAzdoRestClient>();

    [Fact]
    public async Task ShouldReturnInvalidPipelines()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var buildDefinitions = _fixture.CreateMany<BuildDefinition>(1);
        var registrations = _fixture.CreateMany<PipelineRegistration>().ToList();

        var exception = new FlurlHttpException(new HttpCall
        {
            Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            }
        }, "", new Exception());

        _azdoClient
            .Setup(x => x.GetAsync(It.IsAny<IEnumerableRequest<BuildDefinition>>(), organization))
            .ReturnsAsync(buildDefinitions)
            .Verifiable();
        _azdoClient
            .Setup(x => x.PostAsync(It.IsAny<IAzdoRequest<YamlPipelineRequest, YamlPipelineResponse>>(),
                It.IsAny<YamlPipelineRequest>(), organization, It.IsAny<bool>()))
            .ThrowsAsync(exception)
            .Verifiable();

        // Act
        var service = new PipelineService(_azdoClient.Object);
        var allYamlPipelines = await service.GetAllYamlPipelinesAsync(organization, projectId, registrations);

        // Assert
        allYamlPipelines.Count().ShouldBe(1);
        allYamlPipelines.First().PipelineType.ShouldBe(Constants.ItemTypes.InvalidYamlPipeline);
        _azdoClient.Verify();
    }

    [Fact]
    public async Task ShouldReturnPipelinesWithoutStages()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var buildDefinitions = _fixture.CreateMany<BuildDefinition>(1);
        var registrations = _fixture.CreateMany<PipelineRegistration>().ToList();

        var noStageResponse = new YamlPipelineResponse
        {
            Pipeline = new Pipeline { Id = buildDefinitions.First().Id },
            FinalYaml = "stages:\r\n- stage: __default\r\n  jobs:\r\n  - job: Test\r\n\r\n"
        };

        _azdoClient
            .Setup(x => x.GetAsync(It.IsAny<IEnumerableRequest<BuildDefinition>>(), organization))
            .ReturnsAsync(buildDefinitions)
            .Verifiable();
        _azdoClient
            .Setup(x => x.PostAsync(It.IsAny<IAzdoRequest<YamlPipelineRequest, YamlPipelineResponse>>(),
                It.IsAny<YamlPipelineRequest>(), organization, It.IsAny<bool>()))
            .ReturnsAsync(noStageResponse)
            .Verifiable();

        // Act
        var service = new PipelineService(_azdoClient.Object);
        var allYamlPipelines = await service.GetAllYamlPipelinesAsync(organization, projectId, registrations);

        // Assert
        allYamlPipelines.Count().ShouldBe(1);
        allYamlPipelines.First().PipelineType.ShouldBe(Constants.ItemTypes.StagelessYamlPipeline);
        _azdoClient.Verify();
    }

    [Fact]
    public async Task ShouldReturnPipelinesWith1Stage()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var buildDefinition = _fixture.Create<BuildDefinition>();
        buildDefinition.QueueStatus = "enabled";
        var registrations = _fixture.CreateMany<PipelineRegistration>().ToList();

        var singleStageResponse = new YamlPipelineResponse
        {
            Pipeline = new Pipeline { Id = buildDefinition.Id },
            FinalYaml = "stages:\r\n- stage: Build\r\n  jobs:\r\n  - job: Test\r\n\r\n"
        };

        _azdoClient
            .Setup(x => x.GetAsync(It.IsAny<IEnumerableRequest<BuildDefinition>>(), organization))
            .ReturnsAsync(new[] { buildDefinition })
            .Verifiable();
        _azdoClient
            .Setup(x => x.PostAsync(It.IsAny<IAzdoRequest<YamlPipelineRequest, YamlPipelineResponse>>(),
                It.IsAny<YamlPipelineRequest>(), organization, It.IsAny<bool>()))
            .ReturnsAsync(singleStageResponse)
            .Verifiable();

        // Act
        var service = new PipelineService(_azdoClient.Object);
        var allYamlPipelines = await service.GetAllYamlPipelinesAsync(organization, projectId, registrations);

        // Assert
        allYamlPipelines.Count().ShouldBe(1);
        allYamlPipelines.First().PipelineType.ShouldBe(Constants.ItemTypes.YamlPipelineWithStages);
        allYamlPipelines.First().Stages.Count().ShouldBe(1);
        _azdoClient.Verify();
    }

    [Fact]
    public async Task ShouldReturnPipelinesWithMoreThan1Stages()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var buildDefinition = _fixture.Create<BuildDefinition>();
        buildDefinition.QueueStatus = "enabled";
        var registrations = _fixture.CreateMany<PipelineRegistration>().ToList();


        var multiStageResponse = new YamlPipelineResponse
        {
            Pipeline = new Pipeline { Id = buildDefinition.Id },
            FinalYaml = "resources:\r\n  pipelines:\r\n    - pipeline: test\r\nstages:\r\n- " +
                        "stage: Build\r\n  jobs:\r\n  - job: Test\r\n\r\n- stage: Prod\r\n  jobs:\r\n  - job: Test\r\n"
        };

        _azdoClient
            .Setup(x => x.GetAsync(It.IsAny<IEnumerableRequest<BuildDefinition>>(), organization))
            .ReturnsAsync(new[] { buildDefinition })
            .Verifiable();
        _azdoClient
            .Setup(x => x.PostAsync(It.IsAny<IAzdoRequest<YamlPipelineRequest, YamlPipelineResponse>>(),
                It.IsAny<YamlPipelineRequest>(), organization, It.IsAny<bool>()))
            .ReturnsAsync(multiStageResponse)
            .Verifiable();

        // Act
        var service = new PipelineService(_azdoClient.Object);
        var allYamlPipelines = await service.GetAllYamlPipelinesAsync(organization, projectId, registrations);

        // Assert
        allYamlPipelines.Count().ShouldBe(1);
        allYamlPipelines.First().PipelineType.ShouldBe(Constants.ItemTypes.YamlPipelineWithStages);
        allYamlPipelines.First().Stages.Count().ShouldBe(2);
        _azdoClient.Verify();
    }

    [Fact]
    public async Task ShouldReturnDisabledPipelines()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var buildDefinition = _fixture.Create<BuildDefinition>();
        buildDefinition.QueueStatus = "disabled";
        var registrations = _fixture.CreateMany<PipelineRegistration>().ToList();


        _azdoClient
            .Setup(x => x.GetAsync(It.IsAny<IEnumerableRequest<BuildDefinition>>(), organization))
            .ReturnsAsync(new[] { buildDefinition })
            .Verifiable();

        // Act
        var service = new PipelineService(_azdoClient.Object);
        var allYamlPipelines = await service.GetAllYamlPipelinesAsync(organization, projectId, registrations);

        // Assert
        allYamlPipelines.Count().ShouldBe(1);
        allYamlPipelines.First().PipelineType.ShouldBe(Constants.ItemTypes.DisabledYamlPipeline);
        _azdoClient.Verify();
    }

    [Theory]
    [InlineData("YamlPipelineWithUnescapedCharacters.txt", 3)]
    [InlineData("YamlPipelineWithoutUnescapedCharacters.txt", 2)]
    public async Task ShouldReturnPipelinesWithUnescapedCharacters(string fileName, int expectedNumberOfStages)
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var buildDefinition = _fixture.Create<BuildDefinition>();
        buildDefinition.QueueStatus = "enabled";
        var registrations = _fixture.CreateMany<PipelineRegistration>().ToList();

        var multiStageResponse = new YamlPipelineResponse
        {
            Pipeline = new Pipeline { Id = buildDefinition.Id },
            FinalYaml = GetYamlFromFile(fileName)
        };

        _azdoClient
            .Setup(x => x.GetAsync(It.IsAny<IEnumerableRequest<BuildDefinition>>(), organization))
            .ReturnsAsync(new[] { buildDefinition })
            .Verifiable();
        _azdoClient
            .Setup(x => x.PostAsync(It.IsAny<IAzdoRequest<YamlPipelineRequest, YamlPipelineResponse>>(),
                It.IsAny<YamlPipelineRequest>(), organization, It.IsAny<bool>()))
            .ReturnsAsync(multiStageResponse)
            .Verifiable();

        // Act
        var service = new PipelineService(_azdoClient.Object);
        var allYamlPipelines = await service.GetAllYamlPipelinesAsync(organization, projectId, registrations);

        // Assert
        allYamlPipelines.Count().ShouldBe(1);
        allYamlPipelines.First().Stages.Count().ShouldBe(expectedNumberOfStages);
        _azdoClient.Verify();
    }

    [Fact]
    public async Task GetClassicReleasePipelinesAsync_UsesCacheCorrectly()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId1 = _fixture.Create<string>();
        var projectId2 = _fixture.Create<string>();
        var pipelineId = _fixture.Create<string>();

        var releaseDefinitions1 = _fixture.CreateMany<ReleaseDefinition>().ToList();
        var classicReleasePipeline = _fixture.Create<ReleaseDefinition>();
        classicReleasePipeline.PipelineRegistrations = null;
        classicReleasePipeline.Id = pipelineId;
        releaseDefinitions1.Add(classicReleasePipeline);

        var releaseDefinitions2 = _fixture.CreateMany<ReleaseDefinition>();
        var randomPipelineId = releaseDefinitions2.First().Id;

        var registration = _fixture.Create<PipelineRegistration>();
        registration.PipelineId = pipelineId;
        registration.PipelineType = Constants.ItemTypes.ClassicReleasePipeline;
        var verifyableCiIdetifier = registration.CiIdentifier;
        var registrations = _fixture.CreateMany<PipelineRegistration>().ToList();
        registrations.Add(registration);

        _azdoClient.Setup(x => x.GetAsync(It.Is<IEnumerableRequest<ReleaseDefinition>>(request => request.Request.Resource.Contains(projectId1)), organization))
            .ReturnsAsync(releaseDefinitions1);

        _azdoClient.Setup(x => x.GetAsync(It.Is<IEnumerableRequest<ReleaseDefinition>>(request => request.Request.Resource.Contains(projectId2)), organization))
            .ReturnsAsync(releaseDefinitions2);

        // Act
        var service = new PipelineService(_azdoClient.Object);
        var pipelines1 = await service.GetClassicReleasePipelinesAsync(organization, projectId1, registrations); // Collect the classic pipelines twice
        var pipelines2 = await service.GetClassicReleasePipelinesAsync(organization, projectId1, registrations);

        // Assert, act again, assert again
        Assert.Equal(verifyableCiIdetifier, pipelines1.First(pipeline => pipeline.Id == pipelineId).PipelineRegistrations.First().CiIdentifier);
        Assert.Equal(verifyableCiIdetifier, pipelines2.First(pipeline => pipeline.Id == pipelineId).PipelineRegistrations.First().CiIdentifier); // This step ensures the pipelineregistrations, which are added later in the mothod, are still there, even if the pipelines come from the cache
        _azdoClient.Verify(x => x.GetAsync(It.Is<IEnumerableRequest<ReleaseDefinition>>(request => request.Request.Resource.Contains(projectId1)), organization), Times.Once); // Azdoclient only called once, because it was cached the first time

        _ = await service.GetClassicReleasePipelinesAsync(organization, projectId2, registrations);

        _azdoClient.Verify(x => x.GetAsync(It.Is<IEnumerableRequest<ReleaseDefinition>>(request => request.Request.Resource.Contains(projectId2)), organization), Times.Once);
    }

    [Fact]
    public async Task GetClassicReleasePipelinesAsync_AlsoFetchesPipelines_WithoutRegistrations()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId1 = _fixture.Create<string>();
        var pipelineId = _fixture.Create<string>();

        var releaseDefinitions1 = _fixture.CreateMany<ReleaseDefinition>();
        foreach (var releaseDefinition in releaseDefinitions1)
        {
            releaseDefinition.PipelineRegistrations = null;
        }

        _azdoClient.Setup(x => x.GetAsync(It.Is<IEnumerableRequest<ReleaseDefinition>>(request => request.Request.Resource.Contains(projectId1)), organization))
            .ReturnsAsync(releaseDefinitions1);

        // Act
        var service = new PipelineService(_azdoClient.Object);
        var pipelines1 = await service.GetClassicReleasePipelinesAsync(organization, projectId1, null);

        // Assert
        Assert.NotNull(pipelines1);
        Assert.NotEmpty(pipelines1);
    }

    [Fact]
    public async Task GetAllYamlPipelinesAsync_UsesCacheCorrectly()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId1 = _fixture.Create<string>();
        var projectId2 = _fixture.Create<string>();
        var pipelineId = _fixture.Create<string>();

        var releaseDefinitions1 = _fixture.CreateMany<BuildDefinition>().ToList();
        var classicReleasePipeline = _fixture.Create<BuildDefinition>();
        classicReleasePipeline.PipelineRegistrations = null;
        classicReleasePipeline.Id = pipelineId;
        releaseDefinitions1.Add(classicReleasePipeline);

        var releaseDefinitions2 = _fixture.CreateMany<BuildDefinition>();
        var randomPipelineId = releaseDefinitions2.First().Id;

        var registration = _fixture.Create<PipelineRegistration>();
        registration.PipelineId = pipelineId;
        registration.PipelineType = Constants.ItemTypes.YamlReleasePipeline;
        var verifyableCiIdetifier = registration.CiIdentifier;
        var registrations = _fixture.CreateMany<PipelineRegistration>().ToList();
        registrations.Add(registration);

        _azdoClient.Setup(x => x.GetAsync(It.Is<IEnumerableRequest<BuildDefinition>>(request => request.Request.Resource.Contains(projectId1)), organization))
            .ReturnsAsync(releaseDefinitions1);

        _azdoClient.Setup(x => x.GetAsync(It.Is<IEnumerableRequest<BuildDefinition>>(request => request.Request.Resource.Contains(projectId2)), organization))
            .ReturnsAsync(releaseDefinitions2);

        // Act
        var service = new PipelineService(_azdoClient.Object);
        var pipelines1 = await service.GetAllYamlPipelinesAsync(organization, projectId1, registrations); // Collect the classic pipelines twice
        var pipelines2 = await service.GetAllYamlPipelinesAsync(organization, projectId1, registrations);

        // Assert, act again, assert again
        Assert.Equal(verifyableCiIdetifier, pipelines1.First(pipeline => pipeline.Id == pipelineId).PipelineRegistrations.First().CiIdentifier);
        Assert.Equal(verifyableCiIdetifier, pipelines2.First(pipeline => pipeline.Id == pipelineId).PipelineRegistrations.First().CiIdentifier); // This step ensures the pipelineregistrations, which are added later in the mothod, are still there, even if the pipelines come from the cache
        _azdoClient.Verify(x => x.GetAsync(It.Is<IEnumerableRequest<BuildDefinition>>(request => request.Request.Resource.Contains(projectId1)), organization), Times.Once); // Azdoclient only called once, because it was cached the first time

        _ = await service.GetAllYamlPipelinesAsync(organization, projectId2, registrations);

        _azdoClient.Verify(x => x.GetAsync(It.Is<IEnumerableRequest<BuildDefinition>>(request => request.Request.Resource.Contains(projectId2)), organization), Times.Once);
    }

    [Fact]
    public async Task GetClassicBuildPipelinesAsync_UsesCacheCorrectly()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId1 = _fixture.Create<string>();
        var projectId2 = _fixture.Create<string>();
        var pipelineId = _fixture.Create<string>();

        var releaseDefinitions1 = _fixture.CreateMany<BuildDefinition>().ToList();
        var classicReleasePipeline = _fixture.Create<BuildDefinition>();
        classicReleasePipeline.PipelineRegistrations = null;
        classicReleasePipeline.Id = pipelineId;
        releaseDefinitions1.Add(classicReleasePipeline);

        var releaseDefinitions2 = _fixture.CreateMany<BuildDefinition>();
        var randomPipelineId = releaseDefinitions2.First().Id;

        _azdoClient.Setup(x => x.GetAsync(It.Is<IEnumerableRequest<BuildDefinition>>(request => request.Request.Resource.Contains(projectId1)), organization))
            .ReturnsAsync(releaseDefinitions1);

        _azdoClient.Setup(x => x.GetAsync(It.Is<IEnumerableRequest<BuildDefinition>>(request => request.Request.Resource.Contains(projectId2)), organization))
            .ReturnsAsync(releaseDefinitions2);

        // Act
        var service = new PipelineService(_azdoClient.Object);
        var pipelines1 = await service.GetClassicBuildPipelinesAsync(organization, projectId1); // Collect the classic pipelines twice
        var pipelines2 = await service.GetClassicBuildPipelinesAsync(organization, projectId1);

        // Assert, act again, assert again
        Assert.Equal(Constants.ItemTypes.ClassicBuildPipeline, pipelines1.First(pipeline => pipeline.Id == pipelineId).PipelineType);
        Assert.Equal(Constants.ItemTypes.ClassicBuildPipeline, pipelines2.First(pipeline => pipeline.Id == pipelineId).PipelineType); // This step ensures the pipelinetypes, which are added later in the mothod, are still there, even if the pipelines come from the cache
        _azdoClient.Verify(x => x.GetAsync(It.Is<IEnumerableRequest<BuildDefinition>>(request => request.Request.Resource.Contains(projectId1)), organization), Times.Once); // Azdoclient only called once, because it was cached the first time

        _ = await service.GetClassicBuildPipelinesAsync(organization, projectId2);

        _azdoClient.Verify(x => x.GetAsync(It.Is<IEnumerableRequest<BuildDefinition>>(request => request.Request.Resource.Contains(projectId2)), organization), Times.Once);
    }


    private static string GetYamlFromFile(string fileName)
    {
        var path = System.IO.Path.Combine("Assets", fileName);
        return System.IO.File.ReadAllText(path);
    }

    internal static T GetInstanceField<T>(Type type, object instance, string fieldName)
    {
        BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                 | BindingFlags.Static;
        FieldInfo field = type.GetField(fieldName, bindFlags);
        return (T)field.GetValue(instance);
    }
}