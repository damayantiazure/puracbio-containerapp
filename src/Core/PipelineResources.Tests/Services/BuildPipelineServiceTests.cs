using MemoryCache.Testing.Moq;
using Microsoft.Extensions.Caching.Memory;
using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.PipelineResources.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using static Rabobank.Compliancy.Infra.AzdoClient.Requests.YamlPipeline;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.PipelineResources.Tests.Services;

public class BuildPipelineServiceTests
{
    private readonly IFixture _fixture = new Fixture() { RepeatCount = 1 };
    private readonly IMemoryCache _cache = Create.MockedMemoryCache();
    private readonly Mock<IAzdoRestClient> _azdoClient = new();
    private readonly Mock<IYamlHelper> _yamlHelper = new Mock<IYamlHelper>();
    private const string DownloadBuildArtifacts = "a433f589-fce1-4460-9ee6-44a624aeb1fb";
    private const string DownloadPipelineArtifacts = "61f2a582-95ae-4948-b34d-a1b3c4f6a737";
    private const string CheckoutStep = "6d15af64-176c-496d-b583-fd2ae21d4df4@1";
    private const string OtherTask = "otherTask";
    private const string Organization = "Organization";

    #region [triggers]
    [Fact]
    public async Task IfPipelineHasNoTriggersNoneShouldBeReturned()
    {
        // Arrange
        var input = _fixture.Build<BuildDefinition>()
            .Without(b => b.Triggers)
            .Create();
        input.Process.Type = 1;

        // Act
        var function = new BuildPipelineService(_azdoClient.Object, null, _yamlHelper.Object);
        var result = await function.GetLinkedPipelinesAsync(Organization, input);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldReportCorrectNumberOfTriggeringPipelines()
    {
        // Arrange

        // Input pipeline with a trigger
        var input = _fixture.Create<BuildDefinition>();
        input.Id = "inputPipeline";
        input.Process.Type = 1;
        input.Triggers.First().TriggerType = "buildCompletion";
        input.Triggers.First().Definition.Id = "triggeringPipeline";

        // Trigger from the input pipeline (also triggered by another pipeline)
        var triggeringPipeline = _fixture.Create<BuildDefinition>();
        triggeringPipeline.Id = "triggeringPipeline";
        triggeringPipeline.Triggers.First().TriggerType = "buildCompletion";
        triggeringPipeline.Triggers.First().Definition.Id = "AnotherTriggeringPipeline";
        triggeringPipeline.Process.Type = 1;

        // Trigger from the triggering pipeline
        var anotherTriggeringPipeline = _fixture.Build<BuildDefinition>()
            .Without(b => b.Triggers)
            .Create();
        anotherTriggeringPipeline.Id = "AnotherTriggeringPipeline";
        anotherTriggeringPipeline.Process.Type = 1;

        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<BuildDefinition>>(), It.IsAny<string>()))
            .Returns((
                IAzdoRequest<BuildDefinition> req, string organization) =>
            {
                if (req.Resource.Contains(input.Triggers.First().Definition.Id))
                {
                    return Task.FromResult(triggeringPipeline);
                }

                if (req.Resource.Contains(triggeringPipeline.Triggers.First().Definition.Id))
                {
                    return Task.FromResult(anotherTriggeringPipeline);
                }

                return Task.FromResult<BuildDefinition>(null);
            });

        // Act
        var function = new BuildPipelineService(_azdoClient.Object, null, _yamlHelper.Object);
        var result = await function.GetLinkedPipelinesAsync(Organization, input);

        // Assert
        result.Should().HaveCount(2);
    }
    #endregion

    #region [download artifacts task]
    [Theory]
    [InlineData(DownloadBuildArtifacts, "buildType", "definition")]
    [InlineData(DownloadPipelineArtifacts, "source", "pipeline")]
    public async Task ClassicBuildPipelineWithValidDownloadTaskShouldReturnBuildDefinition(string taskId, string key, string key2)
    {
        //Arrrange
        _fixture.Customize<BuildProcess>(ctx => ctx
            .With(p => p.Type, 1));
        _fixture.Customize<BuildStep>(ctx => ctx
            .With(s => s.Enabled, true)
            .With(t => t.Inputs, new Dictionary<string, string> { [key] = "specific", ["project"] = "1", [key2] = "1" }));
        _fixture.Customize<BuildTask>(ctx => ctx
            .With(t => t.Id, taskId)
            .With(t => t.DefinitionType, "task"));
        _fixture.Customize<BuildDefinition>(ctx => ctx
            .Without(b => b.Yaml)
            .Without(b => b.Triggers));

        var input = _fixture.Create<BuildDefinition>();

        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<BuildDefinition>>(), Organization))
            .ReturnsAsync(_fixture.Create<BuildDefinition>());
        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Project>>(), Organization))
            .ReturnsAsync(_fixture.Create<Project>());

        //Act
        var function = new BuildPipelineService(_azdoClient.Object, _cache, _yamlHelper.Object);
        var result = await function.GetLinkedPipelinesAsync(Organization, input);

        //Assert
        result.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(OtherTask, true, "buildType", "specific", "definition")]
    [InlineData(DownloadBuildArtifacts, false, "buildType", "specific", "definition")]
    [InlineData(DownloadPipelineArtifacts, false, "source", "specific", "pipeline")]
    [InlineData(DownloadBuildArtifacts, true, "buildType", "current", "definition")]
    [InlineData(DownloadPipelineArtifacts, true, "source", "current", "pipeline")]
    public async Task ClassicBuildPipelineWithout_OrWithInvalid_DownloadTaskShouldReturnNull(string taskId, bool enabled, string key, string value, string key2)
    {
        //Arrange
        _fixture.Customize<BuildProcess>(ctx => ctx
            .With(p => p.Type, 1));
        _fixture.Customize<BuildStep>(ctx => ctx
            .With(s => s.Enabled, enabled)
            .With(t => t.Inputs, new Dictionary<string, string> { [key] = value, ["project"] = "1", [key2] = "1" }));
        _fixture.Customize<BuildTask>(ctx => ctx
            .With(t => t.Id, taskId)
            .With(t => t.DefinitionType, "task"));
        _fixture.Customize<BuildDefinition>(ctx => ctx
            .Without(b => b.Yaml)
            .Without(b => b.Triggers));

        var input = _fixture.Create<BuildDefinition>();

        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<BuildDefinition>>(), Organization))
            .ReturnsAsync(_fixture.Create<BuildDefinition>());
        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Project>>(), Organization))
            .ReturnsAsync(_fixture.Create<Project>());

        //Act
        var function = new BuildPipelineService(_azdoClient.Object, _cache, _yamlHelper.Object);
        var result = await function.GetLinkedPipelinesAsync(Organization, input);

        //Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(DownloadBuildArtifacts, "buildType", "definition")]
    [InlineData(DownloadPipelineArtifacts, "source", "pipeline")]
    public async Task ClassicBuildPipelineWithEnabledTaskGroup_WithValidDownloadTask_ShoulReturnBuildDefinition(string taskId, string key, string key2)
    {
        //Arrange
        _fixture.Customize<BuildProcess>(ctx => ctx
            .With(p => p.Type, 1));
        _fixture.Customize<BuildStep>(ctx => ctx
            .With(s => s.Enabled, true));
        _fixture.Customize<BuildTask>(ctx => ctx
            .With(t => t.DefinitionType, "metaTask"));
        _fixture.Customize<BuildDefinition>(ctx => ctx
            .Without(b => b.Yaml)
            .Without(b => b.Triggers));

        var downloadTask = new BuildStep
        {
            Enabled = true,
            Task = new BuildTask
            {
                Id = taskId
            },
            Inputs = new Dictionary<string, string> { [key] = "specific", ["project"] = "1", [key2] = "1" }
        };

        var taskGroup = new TaskGroup { Tasks = new[] { downloadTask } };
        var taskGroupResponse = new TaskGroupResponse { Value = new List<TaskGroup> { taskGroup } };

        var input = _fixture.Create<BuildDefinition>();

        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<TaskGroupResponse>>(), Organization))
            .ReturnsAsync(taskGroupResponse);
        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<BuildDefinition>>(), Organization))
            .ReturnsAsync(_fixture.Create<BuildDefinition>());
        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Project>>(), Organization))
            .ReturnsAsync(_fixture.Create<Project>());

        //Act
        var function = new BuildPipelineService(_azdoClient.Object, _cache, _yamlHelper.Object);
        var result = await function.GetLinkedPipelinesAsync(Organization, input);

        //Assert
        result.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(DownloadBuildArtifacts, false, "buildType", "specific", "definition")]
    [InlineData(DownloadPipelineArtifacts, false, "source", "specific", "pipeline")]
    [InlineData(OtherTask, true, "source", "specific", "pipeline")]
    [InlineData(DownloadBuildArtifacts, true, "buildType", "current", "definition")]
    [InlineData(DownloadPipelineArtifacts, true, "source", "current", "pipeline")]
    public async Task ClassicBuildPipelineWithDisabledTaskGroup_OrTaskgroupWithInvalidDownloadTask_ShoulReturnNullAsync(string taskId, bool enabled, string key, string value, string key2)
    {
        //Arrange
        _fixture.Customize<BuildProcess>(ctx => ctx
            .With(p => p.Type, 1));
        _fixture.Customize<BuildStep>(ctx => ctx
            .With(s => s.Enabled, enabled));
        _fixture.Customize<BuildTask>(ctx => ctx
            .With(t => t.DefinitionType, "metaTask"));
        _fixture.Customize<BuildDefinition>(ctx => ctx
            .Without(b => b.Yaml)
            .Without(b => b.Triggers));

        var downloadTask = new BuildStep
        {
            Enabled = true,
            Task = new BuildTask
            {
                Id = taskId
            },
            Inputs = new Dictionary<string, string> { [key] = value, ["project"] = "1", [key2] = "1" }
        };

        var taskGroup = new TaskGroup { Tasks = new[] { downloadTask } };
        var taskGroupResponse = new TaskGroupResponse { Value = new List<TaskGroup> { taskGroup } };

        var input = _fixture.Create<BuildDefinition>();

        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<TaskGroupResponse>>(), Organization))
            .ReturnsAsync(taskGroupResponse);
        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<BuildDefinition>>(), Organization))
            .ReturnsAsync(_fixture.Create<BuildDefinition>());
        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Project>>(), Organization))
            .ReturnsAsync(_fixture.Create<Project>());

        //Act
        var function = new BuildPipelineService(_azdoClient.Object, _cache, _yamlHelper.Object);
        var result = await function.GetLinkedPipelinesAsync(Organization, input);

        //Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("DownloadBuildArtifacts@1", "buildType", "definition")]
    [InlineData("DownloadPipelineArtifact@2", "source", "pipeline")]
    public async Task YamlPipelineWithValidDownloadTaskShouldReturnBuildDefinitionAsync(string taskName, string key, string key2)
    {
        //Arrange
        _fixture.Customize<BuildProcess>(ctx => ctx
            .With(p => p.Type, 2));
        _fixture.Customize<BuildDefinition>(ctx => ctx
            .Without(b => b.Yaml)
            .Without(b => b.YamlUsedInRun)
            .Without(b => b.Triggers));

        var input = _fixture.Create<BuildDefinition>();

        var yamlResponse = new YamlPipelineResponse { FinalYaml = $@"
            stages:
            - stage:
              jobs:
              - job: JobName
                steps:
                - task: {taskName}
                  inputs:
                    {key}: 'specific'
                    project: '1'
                    {key2}: '1'  " };

        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<BuildDefinition>>(), Organization))
            .ReturnsAsync(_fixture.Create<BuildDefinition>());
        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Project>>(), Organization))
            .ReturnsAsync(_fixture.Create<Project>());
        _azdoClient
            .Setup(c => c.PostAsync(It.IsAny<IAzdoRequest<YamlPipelineRequest, YamlPipelineResponse>>(),
                It.IsAny<YamlPipelineRequest>(), Organization, true))
            .ReturnsAsync(yamlResponse);

        //Act
        var function = new BuildPipelineService(_azdoClient.Object, _cache, _yamlHelper.Object);
        var result = await function.GetLinkedPipelinesAsync(Organization, input);

        //Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task YamlPipelineThatUsesAliasesShouldReturnBuildDefinitionAsync()
    {
        //Arrange
        _fixture.Customize<BuildProcess>(ctx => ctx
            .With(p => p.Type, 2));
        _fixture.Customize<BuildDefinition>(ctx => ctx
            .Without(b => b.Yaml)
            .Without(b => b.YamlUsedInRun)
            .Without(b => b.Triggers));

        var input = _fixture.Create<BuildDefinition>();

        var yamlResponse = new YamlPipelineResponse { FinalYaml = $@"
            stages:
            - stage:
              jobs:
              - job: JobName
                steps:
                - task: DownloadPipelineArtifact@2
                  inputs:
                    buildType: 'specific'
                    project: '1'
                    definition: '1'  " };

        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<BuildDefinition>>(), Organization))
            .ReturnsAsync(_fixture.Create<BuildDefinition>());
        _azdoClient
            .Setup(c => c.PostAsync(It.IsAny<IAzdoRequest<YamlPipelineRequest, YamlPipelineResponse>>(),
                It.IsAny<YamlPipelineRequest>(), Organization, true))
            .ReturnsAsync(yamlResponse);

        //Act
        var function = new BuildPipelineService(_azdoClient.Object, _cache, _yamlHelper.Object);
        var result = await function.GetLinkedPipelinesAsync(Organization, input);

        //Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task YamlPipelineThatUsesPipelineNameInsteadOfIdShouldReturnBuildDefinitionAsync()
    {
        //Arrange
        _fixture.Customize<BuildProcess>(ctx => ctx
            .With(p => p.Type, 2));
        _fixture.Customize<BuildDefinition>(ctx => ctx
            .Without(b => b.Yaml)
            .Without(b => b.YamlUsedInRun)
            .Without(b => b.Triggers));

        var input = _fixture.Create<BuildDefinition>();

        var yamlResponse = new YamlPipelineResponse { FinalYaml = $@"
            stages:
            - stage:
              jobs:
              - job: JobName
                steps:
                - task: DownloadPipelineArtifact@2
                  inputs:
                    source: 'specific'
                    project: '1'
                    pipeline: 'PipelineName'  " };

        var allPipelines = _fixture.Build<BuildDefinition>()
            .With(d => d.Name, "PipelineName")
            .CreateMany(1);

        _azdoClient
            .Setup(c => c.PostAsync(It.IsAny<IAzdoRequest<YamlPipelineRequest, YamlPipelineResponse>>(),
                It.IsAny<YamlPipelineRequest>(), Organization, true))
            .ReturnsAsync(yamlResponse);
        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IEnumerableRequest<BuildDefinition>>(), Organization))
            .ReturnsAsync(allPipelines);

        //Act
        var function = new BuildPipelineService(_azdoClient.Object, _cache, _yamlHelper.Object);
        var result = await function.GetLinkedPipelinesAsync(Organization, input);

        //Assert
        result.Should().HaveCount(1);
    }

    [Theory]
    [InlineData("DownloadBuildArtifacts@1", false, "buildType", "specific", "definition")]
    [InlineData("DownloadPipelineArtifact@2", false, "source", "specific", "pipeline")]
    [InlineData("OtherTask@2", true, "source", "specific", "pipeline")]
    [InlineData("DownloadBuildArtifacts@1", true, "buildType", "current", "definition")]
    [InlineData("DownloadPipelineArtifact@2", true, "source", "current", "pipeline")]
    public async Task YamlPipelineWithout_OrWithInvalid_DownloadTaskShouldReturnNullAsync(string taskName, bool enabled, string key, string value, string key2)
    {
        //Arrange
        _fixture.Customize<BuildProcess>(ctx => ctx
            .With(p => p.Type, 2));
        _fixture.Customize<BuildDefinition>(ctx => ctx
            .Without(b => b.Yaml)
            .Without(b => b.Triggers));

        var input = _fixture.Create<BuildDefinition>();

        var yamlResponse = new YamlPipelineResponse { FinalYaml = $@"
            jobs:
            - job: JobName
              steps:
              - task: {taskName}
                enabled: {enabled}
                  inputs:
                  {key}: {value}
                  project: '1'
                  {key2}: '1'  " };

        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<BuildDefinition>>(), Organization))
            .ReturnsAsync(_fixture.Create<BuildDefinition>());
        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Project>>(), Organization))
            .ReturnsAsync(_fixture.Create<Project>());
        _azdoClient
            .Setup(c => c.PostAsync(It.IsAny<IAzdoRequest<YamlPipelineRequest, YamlPipelineResponse>>(),
                It.IsAny<YamlPipelineRequest>(), Organization, true))
            .ReturnsAsync(yamlResponse);

        //Act
        var function = new BuildPipelineService(_azdoClient.Object, _cache, _yamlHelper.Object);
        var result = await function.GetLinkedPipelinesAsync(Organization, input);

        //Assert
        result.Should().BeEmpty();
    }
    #endregion

    #region [yaml pipeline resources]
    [Fact]
    public async Task IfYamlPipelineHasNoPrecedingPipelinesNoneShouldBeReturned()
    {
        // Arrange
        var input = _fixture.Build<BuildDefinition>()
            .Without(b => b.Triggers)
            .Create();
        input.Process.Type = 2;

        var yamlPipelineResponse = new YamlPipelineResponse
        {
            Pipeline = new Pipeline { Id = input.Id },
            FinalYaml = ""
        };

        _azdoClient
            .Setup(c => c.PostAsync(
                It.IsAny<IAzdoRequest<YamlPipelineRequest,
                    YamlPipelineResponse>>(),
                It.IsAny<YamlPipelineRequest>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(yamlPipelineResponse);

        // Act
        var function = new BuildPipelineService(_azdoClient.Object, null, _yamlHelper.Object);
        var result = await function.GetLinkedPipelinesAsync(Organization, input);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldBeAbleToResolveAllPrecedingPipelines()
    {
        // Arrange

        // BuildDefinition1 - Project1
        // |
        // |- BuildDefinition2 - Project1
        // |  |
        // |  |- BuildDefinition3 - Project1
        // |
        // |- BuildDefinition4 - Project2

        var project1Id = Guid.NewGuid().ToString("D");
        var project2Id = Guid.NewGuid().ToString("D");
        var folderBuildDefinition2 = "folder2";

        var buildDefinition1 = GenerateBuildDefinition(1, project1Id);
        var buildDefinition2 = GenerateBuildDefinition(2, project1Id, folderBuildDefinition2);
        var buildDefinition3 = GenerateBuildDefinition(3, project1Id);
        var buildDefinition4 = GenerateBuildDefinition(4, project2Id);

        var response1 = new YamlPipelineResponse
        {
            Pipeline = new Pipeline { Id = buildDefinition1.Id },
            FinalYaml = $@"
                                resources:
                                  pipelines:
                                  - pipeline: Pipeline2
                                    source: {folderBuildDefinition2}/{buildDefinition2.Name.ToLower()}
                                  - pipeline: Pipeline4
                                    source: {buildDefinition4.Name}
                                    project: {project2Id}"
        };

        var response2 = new YamlPipelineResponse
        {
            Pipeline = new Pipeline { Id = buildDefinition2.Id },
            FinalYaml = $@"
                                resources:
                                  pipelines:
                                  - pipeline: Pipeline3
                                    source: {buildDefinition3.Name.ToUpper()}"
        };

        var response3 = new YamlPipelineResponse
        {
            Pipeline = new Pipeline { Id = buildDefinition3.Id },
            FinalYaml = string.Empty
        };

        var response4 = new YamlPipelineResponse
        {
            Pipeline = new Pipeline { Id = buildDefinition4.Id },
            FinalYaml = string.Empty
        };

        _azdoClient.Setup(
                c => c.PostAsync(
                    It.Is<IAzdoRequest<YamlPipelineRequest, YamlPipelineResponse>>(request =>
                        request.Resource.Contains(project1Id) && request.Resource.Contains(buildDefinition1.Id)),
                    It.IsAny<YamlPipelineRequest>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(response1);

        _azdoClient.Setup(
                c => c.PostAsync(
                    It.Is<IAzdoRequest<YamlPipelineRequest, YamlPipelineResponse>>(request =>
                        request.Resource.Contains(project1Id) && request.Resource.Contains(buildDefinition2.Id)),
                    It.IsAny<YamlPipelineRequest>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(response2);

        _azdoClient.Setup(
                c => c.PostAsync(
                    It.Is<IAzdoRequest<YamlPipelineRequest, YamlPipelineResponse>>(request =>
                        request.Resource.Contains(project1Id) && request.Resource.Contains(buildDefinition3.Id)),
                    It.IsAny<YamlPipelineRequest>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(response3);

        _azdoClient.Setup(
                c => c.PostAsync(
                    It.Is<IAzdoRequest<YamlPipelineRequest, YamlPipelineResponse>>(request =>
                        request.Resource.Contains(project2Id) && request.Resource.Contains(buildDefinition4.Id)),
                    It.IsAny<YamlPipelineRequest>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(response4);


        _azdoClient
            .Setup(c => c.GetAsync(
                It.IsAny<IEnumerableRequest<BuildDefinition>>(), It.IsAny<string>()))
            .Returns((
                IEnumerableRequest<BuildDefinition> req, string organization) =>
            {
                if (req.Request.Resource.Contains(project1Id))
                {
                    return Task.FromResult<IEnumerable<BuildDefinition>>(new[]
                    {
                        buildDefinition1,
                        buildDefinition2,
                        buildDefinition3
                    });
                }

                if (req.Request.Resource.Contains(project2Id))
                {
                    return Task.FromResult<IEnumerable<BuildDefinition>>(new[]
                    {
                        buildDefinition4
                    });
                }

                return Task.FromResult<IEnumerable<BuildDefinition>>(null);
            });

        // Act
        var function = new BuildPipelineService(_azdoClient.Object, null, _yamlHelper.Object);
        var result = await function.GetLinkedPipelinesAsync(Organization, buildDefinition1);

        // Assert
        result.Should().HaveCount(3);
    }

    private BuildDefinition GenerateBuildDefinition(int iteration, string projectId, string buildDefinitionFolder = null)
    {
        var buildDefinition = _fixture.Build<BuildDefinition>()
            .Without(b => b.Triggers)
            .Without(b => b.Yaml)
            .Without(b => b.YamlUsedInRun)
            .Create();
        buildDefinition.Id = $"buildDefinitionId{iteration}";
        buildDefinition.Name = $"buildDefinitionName{iteration}";
        buildDefinition.Project.Id = projectId;
        buildDefinition.Process.Type = 2;

        if (!string.IsNullOrEmpty(buildDefinitionFolder))
        {
            buildDefinition.Path = $"\\{buildDefinitionFolder}";
        }

        return buildDefinition;
    }
    #endregion

    #region [repositories]
    [Fact]
    public async Task ShouldBeAbleToResolveLinkedRepositories()
    {
        // Arrange
        var input = _fixture.Create<BuildDefinition>();
        input.Repository.Type = RepositoryTypes.TfsGit;
        input.Repository.Name = "RepoA";

        // Act
        var function = new BuildPipelineService(null, null, _yamlHelper.Object);
        var repositories = await function.GetLinkedRepositoriesAsync(Organization, new List<BuildDefinition> { input });

        // Assert
        repositories.Should().HaveCount(1);
        repositories.Select(r => r.Name).Should().BeEquivalentTo(new[] { "RepoA" });
    }

    [Theory]
    [InlineData("Repo1", "Repo1")]
    [InlineData("ProjectB/Repo2", "Repo2")]
    public async Task ShouldBeAbleToResolveYamlRepositoryResources(string name, string result)
    {
        // Arrange
        var yaml = $@"
                resources:
                  repositories:
                  - repository: RepoAlias
                    type: git
                    name: {name}";

        var input = _fixture.Build<BuildDefinition>()
            .With(d => d.Yaml, yaml)
            .Without(d => d.YamlUsedInRun)
            .Create();
        input.Project.Id = "Project1";

        var repo1 = _fixture.Build<Repository>()
            .With(r => r.Name, "Repo1")
            .Create();

        var repo2 = _fixture.Build<Repository>()
            .With(r => r.Name, "Repo2")
            .Create();

        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Repository>>(), Organization))
            .Returns((
                IAzdoRequest<Repository> req, string organization) =>
            {
                if (req.Resource.Contains("Project1") && req.Resource.Contains("Repo1"))
                {
                    return Task.FromResult(repo1);
                }

                if (req.Resource.Contains("ProjectB") && req.Resource.Contains("Repo2"))
                {
                    return Task.FromResult(repo2);
                }

                return Task.FromResult<Repository>(null);
            });

        // Act
        var function = new BuildPipelineService(_azdoClient.Object, null, _yamlHelper.Object);
        var repositories = await function.GetLinkedRepositoriesAsync(Organization, new List<BuildDefinition> { input });

        // Assert
        repositories.Should().HaveCount(1);
        repositories.Select(r => r.Name).Should().BeEquivalentTo(new[] { result });
    }

    [Theory]
    [InlineData("github")]
    [InlineData("bitbucket")]
    public async Task ShouldNotReturnRepositoryResourcesOtherThanGit(string type)
    {
        // Arrange
        var yaml = $@"
                resources:
                  repositories:
                  - repository: RepoAlias
                    type: {type}
                    name: MyExternalProject/MyGitHubRepo";

        var input = _fixture.Build<BuildDefinition>()
            .With(d => d.Yaml, yaml)
            .Create();


        // Act
        var function = new BuildPipelineService(_azdoClient.Object, null, _yamlHelper.Object);
        var repositories = await function.GetLinkedRepositoriesAsync(Organization, new List<BuildDefinition> { input });

        // Assert
        repositories.Should().BeEmpty();
    }

    [Fact]
    public async Task IfYamlPipelineHasNoRepositoryResourcesNoneShouldBeReturned()
    {
        // Arrange
        var yaml = $@"
                resources:
                  pipelines:
                  - pipeline: PipelineAlias
                    source: Pipeline1
                    project: ProjectA";

        var input = _fixture.Build<BuildDefinition>()
            .With(d => d.Yaml, yaml)
            .Create();


        // Act
        var function = new BuildPipelineService(_azdoClient.Object, null, _yamlHelper.Object);
        var repositories = await function.GetLinkedRepositoriesAsync(Organization, new List<BuildDefinition> { input });

        // Assert
        repositories.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldReturnRepositoryFromCheckoutStep()
    {
        // Arrange
        var yaml = $@"
                stages:
                - stage:
                  jobs:
                  - job: JobName
                    steps:
                    - task: {CheckoutStep}
                      inputs:
                        repository: git://MyProjectB/MyRepoA
                        path: MyFolder
                    - task: {CheckoutStep}
                      inputs:
                        repository: git://MyRepo";

        var input = _fixture.Build<BuildDefinition>()
            .With(d => d.Yaml, yaml)
            .Without(d => d.YamlUsedInRun)
            .Create();
        input.Project.Id = "Project1";

        var repo1 = _fixture.Build<Repository>()
            .With(r => r.Name, "MyRepo")
            .Create();

        var repo2 = _fixture.Build<Repository>()
            .With(r => r.Name, "MyRepoA")
            .Create();

        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Repository>>(), Organization))
            .Returns((
                IAzdoRequest<Repository> req, string organization) =>
            {
                if (req.Resource.Contains("Project1") && req.Resource.Contains("MyRepo"))
                {
                    return Task.FromResult(repo1);
                }

                if (req.Resource.Contains("ProjectB") && req.Resource.Contains("MyRepoA"))
                {
                    return Task.FromResult(repo2);
                }

                return Task.FromResult<Repository>(null);
            });

        // Act
        var function = new BuildPipelineService(_azdoClient.Object, null, _yamlHelper.Object);
        var repositories = await function.GetLinkedRepositoriesAsync(Organization, new List<BuildDefinition> { input });

        // Assert
        repositories.Should().HaveCount(2);
        repositories.Select(r => r.Name).Should().BeEquivalentTo(new[] { "MyRepoA", "MyRepo" });
    }

    [Theory]
    [InlineData(CheckoutStep, "true", "self")]
    [InlineData(CheckoutStep, "false", "none")]
    [InlineData(CheckoutStep, "true", "github://MyGitHubOrg/MyGitHubRepo")]
    [InlineData(CheckoutStep, "true", "bitbucket://MyBitbucketOrgOrUser/MyBitbucketRepo")]
    [InlineData(DownloadBuildArtifacts, "true", "someRepository")]
    public async Task ShouldNotReturnRepositoryFromInvalidCheckoutStepsOrOtherTasks(string step, string enabled, string repository)
    {
        // Arrange
        var yaml = $@"
                stages:
                - stage:
                  jobs:
                  - job: JobName
                    steps:
                    - task: {step}
                      condition: {enabled}
                      inputs:
                        repository: {repository}";

        var input = _fixture.Build<BuildDefinition>()
            .With(d => d.Yaml, yaml)
            .Create();


        // Act
        var function = new BuildPipelineService(_azdoClient.Object, null, _yamlHelper.Object);
        var repositories = await function.GetLinkedRepositoriesAsync(Organization, new List<BuildDefinition> { input });

        // Assert
        repositories.Should().BeEmpty();
    }

    [Theory]
    [InlineData("git://MyProject/MyRepo@features/tools")]
    [InlineData("git://MyProject/MyRepo@refs/heads/features/tools")]
    [InlineData("git://MyProject/MyRepo@refs/tags/MyTag")]
    public async Task ShouldAlsoReturnRepositoryIfCheckoutStepContainsRef(string item)
    {
        // Arrange
        var yaml = $@"
                stages:
                - stage:
                  jobs:
                  - job: JobName
                    steps:
                    - task: {CheckoutStep}
                      inputs:
                        repository: {item}";

        var input = _fixture.Build<BuildDefinition>()
            .With(d => d.Yaml, yaml)
            .Without(d => d.YamlUsedInRun)
            .Create();

        var repo1 = _fixture.Build<Repository>()
            .With(r => r.Name, "MyRepo")
            .Create();

        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Repository>>(), Organization))
            .Returns((
                IAzdoRequest<Repository> req, string organization) =>
            {
                if (req.Resource.Contains("MyProject") && req.Resource.Contains("MyRepo"))
                {
                    return Task.FromResult(repo1);
                }

                return Task.FromResult<Repository>(null);
            });

        // Act
        var function = new BuildPipelineService(_azdoClient.Object, null, _yamlHelper.Object);
        var repositories = await function.GetLinkedRepositoriesAsync(Organization, new List<BuildDefinition> { input });

        // Assert
        repositories.Select(r => r.Name).Should().BeEquivalentTo(new[] { "MyRepo" });
    }

    [Theory]
    [InlineData("git:///@refs/heads/master")]
    [InlineData("git:///repo@refs/heads/master")]
    [InlineData("git://@refs/heads/master")]
    [InlineData("git://project/repo/fdsfdsf/fsdf@refs/heads/master")]
    [InlineData("git://project/@refs/heads/master")]
    [InlineData("git://project/repo/@refs/heads/master")]
    public async Task GetAsync_InvalidGitUrls_ShouldThrowException(string item)
    {
        // Arrange
        var yaml = $@"
                stages:
                - stage:
                  jobs:
                  - job: JobName
                    steps:
                    - task: {CheckoutStep}
                      inputs:
                        repository: {item}";

        var input = _fixture.Build<BuildDefinition>()
            .With(d => d.Yaml, yaml)
            .Without(d => d.YamlUsedInRun)
            .Create();

        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Repository>>(), Organization))
            .ReturnsAsync(_fixture.Create<Repository>());

        // Act
        var buildPipelineService = new BuildPipelineService(_azdoClient.Object, null, _yamlHelper.Object);

        // Act && Assert
        Func<Task<IEnumerable<Repository>>> getLinkedRepositoriesAction =
            () => buildPipelineService.GetLinkedRepositoriesAsync(Organization, new List<BuildDefinition> { input });

        await getLinkedRepositoriesAction.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Git url: * is not in a valid format");

    }

    #endregion
}