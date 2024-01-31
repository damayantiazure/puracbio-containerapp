using AutoFixture.Kernel;
using AutoMapper;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Domain.Tests.FixtureCustomizations;
using Rabobank.Compliancy.Infrastructure.AzureDevOps;
using Rabobank.Compliancy.Infrastructure.AzureDevOps.Mapping;
using Rabobank.Compliancy.Infrastructure.InternalContracts;
using Rabobank.Compliancy.Infrastructure.Tests.Helpers;
using System.Reflection;
using Artifact = Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts.Artifact;

namespace Rabobank.Compliancy.Infrastructure.Tests;
/* Cases not tested:
    - Stageless
    - Yaml with gitrepo's, should be tested when applicable in functionality (when used in a rule for example) because there might be changes
*/

public class PipelineServiceTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly PipelineService _sut;
    private readonly Mock<IBuildRepository> _buildRepositoryMock = new();
    private readonly Mock<IReleaseRepository> _releaseRepositoryMock = new();
    private readonly Mock<ITaskGroupRepository> _taskGroupRepositoryMock = new();
    private readonly Mock<IGateService> _gateServiceMock = new();
    private readonly Mock<IPipelineRepository> _pipelineRepositoryMock = new();
    private readonly Mock<IProjectService> _projectServiceMock = new();
    private readonly Mock<IGitRepoService> _gitRepoServiceMock = new();
    private readonly IMapper _mapper = CreateMapper();

    public PipelineServiceTests()
    {
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _fixture.Customizations.Add(
            new TypeRelay(
                typeof(PipelineResource),
                typeof(Pipeline))
        );
        _fixture.Customizations.Add(
            new TypeRelay(
                typeof(BuildDefinitionReference),
                typeof(BuildDefinition))
        );

        _fixture.Customizations.Add(
            new TypeRelay(
                typeof(ITrigger),
                typeof(PipelineTrigger)));

        _fixture.Customize<Pipeline>(c => c
            .With(p => p.DefinitionType, PipelineProcessType.Yaml)
        );
        _fixture.Customize(new ProjectWithoutPermissions());
        _fixture.Customize(new IdentityIsAlwaysUser());

        _sut = new PipelineService(_buildRepositoryMock.Object, _releaseRepositoryMock.Object,
            _gateServiceMock.Object, _pipelineRepositoryMock.Object, _projectServiceMock.Object,
            _gitRepoServiceMock.Object, _taskGroupRepositoryMock.Object, _mapper);
    }

    [Fact]
    public async Task GetPipelineAsync_WithCorrectIdInput_ReturnsExpectedResult()
    {
        // Arrange
        const int pipelineId = 10;
        var project = _fixture.Create<Project>();

        InitializeProjectMocks(project);
        var buildDefinitions = CreateDefaultBuildDefinitionList(pipelineId);
        InitializeBuildDefinitionMocks(new Dictionary<Guid, IEnumerable<BuildDefinition>> { { project.Id, buildDefinitions } });

        // Act
        var actual = await _sut.GetPipelineAsync(project, pipelineId, PipelineProcessType.Yaml);

        // Assert
        actual.Id.Should().Be(pipelineId);
    }

    [Fact]
    public async Task GetPipelineAsync_WhenCalledTwice_UsesCache()
    {
        // Arrange
        const int pipelineId = 10;
        var project = _fixture.Create<Project>();

        InitializeProjectMocks(project);
        var buildDefinitions = CreateDefaultBuildDefinitionList(pipelineId);
        InitializeBuildDefinitionMocks(new Dictionary<Guid, IEnumerable<BuildDefinition>> { { project.Id, buildDefinitions } });

        // Act
        var actual1 = await _sut.GetPipelineAsync(project, pipelineId, PipelineProcessType.Yaml);
        var actual2 = await _sut.GetPipelineAsync(project, pipelineId, PipelineProcessType.Yaml);

        // Assert
        actual1.Id.Should().Be(pipelineId).And.Be(actual2.Id);
        _buildRepositoryMock.Verify(x => x.GetBuildDefinitionsByProjectAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetReleaseAsync_WithCorrectIdInput_ReturnsCorrectlyMappedPipeline()
    {
        // Arrange
        var pipelineIds = new[] { 10 };
        var projects = new[] { _fixture.Create<Project>() };
        var firstMockedPipeline = InitializeReleaseDefinitionMocks(pipelineIds, projects)[projects[0]].First();

        // Act
        var actual = await _sut.GetPipelineAsync(projects[0], pipelineIds[0], PipelineProcessType.DesignerRelease);

        // Assert
        actual.Id.Should().Be(firstMockedPipeline.Id);
        actual.DefinitionType.Should().Be(PipelineProcessType.DesignerRelease);
        actual.Name.Should().Be(firstMockedPipeline.Name);
        actual.Path.Should().Be(firstMockedPipeline.Path);
    }

    [Fact]
    public async Task GetReleaseAsync_WhenCalledTwice_UsesCache()
    {
        // Arrange
        var pipelineIds = new[] { 10 };
        var projects = new[] { _fixture.Create<Project>() };
        InitializeReleaseDefinitionMocks(pipelineIds, projects);

        // Act
        var actual1 = await _sut.GetPipelineAsync(projects[0], pipelineIds[0], PipelineProcessType.DesignerRelease);
        var actual2 = await _sut.GetPipelineAsync(projects[0], pipelineIds[0], PipelineProcessType.DesignerRelease);

        // Assert
        actual1.Id.Should().Be(pipelineIds[0]).And.Be(actual2.Id);
        _releaseRepositoryMock.Verify(x => x.GetReleaseDefinitionsByProjectAsync(projects[0].Organization, projects[0].Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPipelineAsync_WithNoResult_ThrowsSourceItemNotFoundException()
    {
        // Act
        var act = async () => await _sut.GetPipelineAsync(_fixture.Create<Project>(), _fixture.Create<int>(), PipelineProcessType.DesignerRelease);

        // Assert
        await act.Should().ThrowAsync<SourceItemNotFoundException>();
    }

    [Fact]
    public async Task GetPipelineAsync_HttpRequestExceptionWithNotFoundStatusCode_ThrowsSourceItemNotFoundException()
    {
        // Arrange
        _releaseRepositoryMock.Setup(r => r.GetReleaseDefinitionsByProjectAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException(_fixture.Create<string>(), null, HttpStatusCode.NotFound));

        // Act
        var act = async () => await _sut.GetPipelineAsync(_fixture.Create<Project>(), _fixture.Create<int>(), PipelineProcessType.DesignerRelease);

        // Assert
        await act.Should().ThrowAsync<SourceItemNotFoundException>();
    }

    [Fact]
    public async Task GetPipelineAsync_HttpRequestExceptionWithBadRequestStatusCode_ThrowsHttpRequestException()
    {
        // Arrange
        _releaseRepositoryMock.Setup(r => r.GetReleaseDefinitionsByProjectAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException(_fixture.Create<string>(), null, HttpStatusCode.BadRequest));

        // Act
        var act = async () => await _sut.GetPipelineAsync(_fixture.Create<Project>(), _fixture.Create<int>(), PipelineProcessType.DesignerRelease);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetPipelineAsync_RandomException_ThrowsSameException()
    {
        // Arrange
        _releaseRepositoryMock.Setup(r => r.GetReleaseDefinitionsByProjectAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException());

        // Act
        var act = async () => await _sut.GetPipelineAsync(_fixture.Create<Project>(), _fixture.Create<int>(), PipelineProcessType.DesignerRelease);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetSinglePipelineForScanAsync_WithCorrectReleaseInput_ReturnsExpectedResult()
    {
        // Arrange
        var pipelineIds = new[] { 10 };
        var projects = new[] { _fixture.Create<Project>() };
        InitializeReleaseDefinitionMocks(pipelineIds, projects);

        // Act
        var actual = await _sut.GetSinglePipelineForScanAsync(projects[0], pipelineIds[0], PipelineProcessType.DesignerRelease);

        // Assert
        actual.Id.Should().Be(pipelineIds[0]);
        actual.DefaultRunContent?.Gates.Should().HaveCount(1);
        actual.DefaultRunContent?.Stages.Should().HaveCount(1);
        actual.DefaultRunContent?.Resources.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetSinglePipelineForScanAsync_WhenCalledTwiceWithDifferentProjects_UsesCacheForDifferentProjects()
    {
        // Arrange
        var pipelineIds = new[] { 11, 12, 13 };
        var projects = new[] { _fixture.Create<Project>(), _fixture.Create<Project>() };
        InitializeReleaseDefinitionMocks(pipelineIds, projects);

        // Act
        var actual1 = await _sut.GetSinglePipelineForScanAsync(projects[0], pipelineIds[0], PipelineProcessType.DesignerRelease);
        var actual2 = await _sut.GetSinglePipelineForScanAsync(projects[0], pipelineIds[1], PipelineProcessType.DesignerRelease);
        var actual3 = await _sut.GetSinglePipelineForScanAsync(projects[1], pipelineIds[2], PipelineProcessType.DesignerRelease);
        var actual4 = await _sut.GetSinglePipelineForScanAsync(projects[1], pipelineIds[2], PipelineProcessType.DesignerRelease);

        // Assert
        actual1.Id.Should().Be(pipelineIds[0]).And.NotBe(actual2.Id); // The first two pipelines exist in the first project and are different
        actual3.Id.Should().Be(pipelineIds[2]).And.Be(actual4.Id); // The third exists alone in the second project, so actual 3 and 4 are the same

        // The following mock should have been called twice. Once for each project.
        _releaseRepositoryMock.Verify(x => x.GetReleaseDefinitionsByProjectAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetSinglePipelineForScanAsync_WithLinkedDownloadTaskUsingBuildDefinitionType_ShouldReturnSingleLinkedPipeline()
    {
        // Arrange
        var consumedProjectId = Guid.Parse("106f7e65-65cc-45a1-980f-a90e414ec820"); // This is the Guid as referenced in the download task in the Yaml. We will use this as the default project for this test, no need to test a separate-project-case.
        var project = _fixture.Build<Project>().With(x => x.Id, consumedProjectId).Create();
        var pipelineId = _fixture.Create<int>();
        const int consumedPipelineId = 1258; // same ID as in the Resources/PipelineWithStagesAndDownloadTask.yml

        InitializeProjectMocks(project);

        var yamlProcess = _fixture.Create<YamlProcess>();
        var buildDefinitions = _fixture.Build<BuildDefinition>()
            .With(x => x.Process, yamlProcess)
            .Without(x => x.AuthoredBy)
            .CreateMany(2).ToList();

        buildDefinitions[0].Id = pipelineId;
        buildDefinitions[1].Id = consumedPipelineId;
        InitializeBuildDefinitionMocks(new Dictionary<Guid, IEnumerable<BuildDefinition>> { { project.Id, buildDefinitions } }, customYamlForFirstBuildDefinition: ResourceFileHelper.GetContentFromResourceFile("PipelineWithStagesAndDownloadTask.yml"));

        // Act
        var actual = await _sut.GetSinglePipelineForScanAsync(project, pipelineId, PipelineProcessType.Yaml);

        // Assert
        Assert.Single(actual.ConsumedResources!);
        Assert.Equal(consumedPipelineId, ((Pipeline?)actual.ConsumedResources!.FirstOrDefault())?.Id);
    }

    [Fact]
    public async Task GetSinglePipelineForScanAsync_WithLinkedMainframeCobolTaskUsingBuildDefinitionType_ShouldReturnSingleLinkedPipeline()
    {
        // Arrange
        var consumedProjectId = Guid.Parse("555efb82-c8b1-4866-a301-9e6dab68d734"); // This is the Guid as referenced in the mainframe cobol task in the Yaml. We will use this as the default project for this test, no need to test a separate-project-case.
        var project = _fixture.Build<Project>().With(x => x.Id, consumedProjectId).Create();
        var pipelineId = _fixture.Create<int>();
        const int consumedPipelineId = 1239; // same ID as in the Resources/PipelineWithStagesAndMainFrameCobolTask.yml

        InitializeProjectMocks(project);

        var yamlProcess = _fixture.Create<YamlProcess>();
        var buildDefinitions = _fixture.Build<BuildDefinition>()
            .With(x => x.Process, yamlProcess)
            .Without(x => x.AuthoredBy)
            .CreateMany(2).ToList();

        buildDefinitions[0].Id = pipelineId;
        buildDefinitions[1].Id = consumedPipelineId;
        InitializeBuildDefinitionMocks(new Dictionary<Guid, IEnumerable<BuildDefinition>> { { project.Id, buildDefinitions } }, customYamlForFirstBuildDefinition: ResourceFileHelper.GetContentFromResourceFile("PipelineWithStagesAndMainFrameCobolTask.yml"));

        // Act
        var actual = await _sut.GetSinglePipelineForScanAsync(project, pipelineId, PipelineProcessType.Yaml);

        // Assert
        Assert.Single(actual.ConsumedResources!);
        Assert.Equal(consumedPipelineId, ((Pipeline?)actual.ConsumedResources!.FirstOrDefault())?.Id);
    }

    [Fact]
    public async Task GetSinglePipelineForScanAsync_WithLinkedTriggeredUsingBuildDefinitionType_ShouldReturnSingleLinkedPipeline()
    {
        // Arrange
        var project = _fixture.Create<Project>();
        var pipelineId = _fixture.Create<int>();
        var consumedPipelineId = _fixture.Create<int>();

        var triggers = new List<BuildTrigger>
        {
            new BuildCompletionTrigger
            {
                Definition = new DefinitionReference
                {
                    Id = consumedPipelineId,
                    Project = new Microsoft.TeamFoundation.Core.WebApi.TeamProjectReference
                    {
                        Id = project.Id
                    }
                }
            }
        };

        var yamlProcess = _fixture.Create<YamlProcess>();
        var buildDefinitions = _fixture.Build<BuildDefinition>()
            .With(x => x.Process, yamlProcess)
            .Without(x => x.AuthoredBy)
            .CreateMany(2).ToList();

        buildDefinitions[0].Id = pipelineId;
        buildDefinitions[1].Id = consumedPipelineId;

        SetCustomTriggers(triggers, buildDefinitions[0]);

        InitializeProjectMocks(project);
        InitializeBuildDefinitionMocks(new Dictionary<Guid, IEnumerable<BuildDefinition>> { { project.Id, buildDefinitions } });

        // Act
        var actual = await _sut.GetSinglePipelineForScanAsync(project, pipelineId, PipelineProcessType.Yaml);

        // Assert
        Assert.Single(actual.ConsumedResources!);
        Assert.Equal(consumedPipelineId, ((Pipeline?)actual.ConsumedResources!.FirstOrDefault())?.Id);
    }

    [Fact]
    public async Task GetSinglePipelineForScanAsync_WithLinkedResourcesBuildDefinitionType_ShouldReturnSingleLinkedPipeline()
    {
        // Arrange
        var pipelineId = _fixture.Create<int>();
        const int consumedPipelineId = 1239; // same ID as in the Resources/PipelineWithStagesAndMainFrameCobolTask.yml
        const string consumedProjectName = "Fabrikam"; // same ID as in the Resources/PipelineWithStagesAndMainFrameCobolTask.yml

        var project = _fixture.Create<Project>();
        var consumedProject = _fixture.Build<Project>().With(x => x.Name, consumedProjectName).Create();

        InitializeProjectMocks(project);
        InitializeProjectMocks(consumedProject);

        var yamlProcess = _fixture.Create<YamlProcess>();
        var buildDefinition = _fixture.Build<BuildDefinition>()
            .With(x => x.Process, yamlProcess)
            .With(x => x.Id, pipelineId)
            .Without(x => x.AuthoredBy)
            .Create();

        var consumedBuildDefinition = _fixture.Build<BuildDefinition>()
            .With(x => x.Process, yamlProcess)
            .With(x => x.Id, consumedPipelineId)
            .With(x => x.Name, "Farbrikam-CI") // same as in Yaml, this one is fetched by name
            .Without(x => x.AuthoredBy)
            .Create();

        var buildDefinitionsPerProject = new Dictionary<Guid, IEnumerable<BuildDefinition>>
        {
            { project.Id, new[] { buildDefinition } },
            { consumedProject.Id, new[] { consumedBuildDefinition } }
        };

        InitializeBuildDefinitionMocks(buildDefinitionsPerProject, customYamlForFirstBuildDefinition: ResourceFileHelper.GetContentFromResourceFile("BuildPipelineMultistageWithResource.yml"));

        // Act
        var actual = await _sut.GetSinglePipelineForScanAsync(project, pipelineId, PipelineProcessType.Yaml);

        // Assert
        Assert.Single(actual.ConsumedResources!);
        Assert.Equal(consumedPipelineId, ((Pipeline?)actual.ConsumedResources!.FirstOrDefault())?.Id);
    }

    [Fact]
    public async Task GetSinglePipelineForScanAsync_WithGitRepoAsResource_ShouldReturnPipelineWithGitRepo()
    {
        // Arrange
        var pipelineId = _fixture.Create<int>();
        // Name as used in BuildPipelineWithGitRepoResource.yml resource file
        const string gitRepositoryName = "CommonTools";
        var project = _fixture.Create<Project>();

        var yamlProcess = _fixture.Create<YamlProcess>();
        var buildDefinition = _fixture.Build<BuildDefinition>()
            .With(x => x.Process, yamlProcess)
            .With(x => x.Id, pipelineId)
            .Without(x => x.AuthoredBy)
            .Create();

        var buildDefinitionsPerProject = new Dictionary<Guid, IEnumerable<BuildDefinition>>
        {
            { project.Id, new[] { buildDefinition } },
        };

        InitializeBuildDefinitionMocks(buildDefinitionsPerProject, customYamlForFirstBuildDefinition:
            ResourceFileHelper.GetContentFromResourceFile("BuildPipelineWithGitRepoResource.yml"));

        _gitRepoServiceMock.Setup(m => m.GetGitRepoByNameAsync(It.IsAny<Project>(), gitRepositoryName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GitRepo { Id = Guid.NewGuid(), Name = gitRepositoryName });

        // Act
        var actual = await _sut.GetSinglePipelineForScanAsync(project, pipelineId, PipelineProcessType.Yaml);

        // Assert
        Assert.Equal(gitRepositoryName, actual.DefaultRunContent!.Resources!.Single().Name);
    }

    [Fact]
    public async Task GetPipelineAsync_WithGenericForBuildDefinition_ReturnsPipelineOfCorrectType()
    {
        // Arrange
        const int pipelineId = 10;
        var project = _fixture.Create<Project>();

        InitializeProjectMocks(project);
        var buildDefinitions = CreateDefaultBuildDefinitionList(pipelineId);
        InitializeBuildDefinitionMocks(new Dictionary<Guid, IEnumerable<BuildDefinition>> { { project.Id, buildDefinitions } });

        // Act
        var actual = await _sut.GetPipelineAsync<AzdoBuildDefinitionPipeline>(project, pipelineId);

        // Assert
        actual.Should().BeOfType<AzdoBuildDefinitionPipeline>();
    }

    [Fact]
    public async Task GetPipelineAsync_WithGenericForReleaseDefinition_ReturnsPipelineOfCorrectType()
    {
        // Arrange
        var pipelineIds = new[] { 10 };
        var projects = new[] { _fixture.Create<Project>() };
        InitializeReleaseDefinitionMocks(pipelineIds, projects);

        // Act
        var actual = await _sut.GetPipelineAsync<AzdoReleaseDefinitionPipeline>(projects[0], pipelineIds[0]);

        // Assert
        actual.Should().BeOfType<AzdoReleaseDefinitionPipeline>();
    }


    private void InitializeBuildDefinitionMocks(Dictionary<Guid, IEnumerable<BuildDefinition>> buildDefinitionsPerProject, string? customYamlForFirstBuildDefinition = null)
    {
        foreach (var kvp in buildDefinitionsPerProject)
        {
            _buildRepositoryMock.Setup(x => x.GetBuildDefinitionsByProjectAsync(It.IsAny<string>(), kvp.Key, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(kvp.Value);
        }

        if (!string.IsNullOrEmpty(customYamlForFirstBuildDefinition))
        {
            // The first one will return the custom yaml
            _pipelineRepositoryMock.Setup(x => x.GetPipelineYamlFromPreviewRunAsync(It.IsAny<string>(), It.IsAny<Guid>(), buildDefinitionsPerProject.First().Value.First().Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(customYamlForFirstBuildDefinition);

            // The rest will return the default
            _pipelineRepositoryMock.Setup(x => x.GetPipelineYamlFromPreviewRunAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.Is<int>(i => i != buildDefinitionsPerProject.First().Value.First().Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ResourceFileHelper.GetContentFromResourceFile("BuildPipeline.yml"));
        }
        else
        {
            _pipelineRepositoryMock.Setup(x => x.GetPipelineYamlFromPreviewRunAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ResourceFileHelper.GetContentFromResourceFile("BuildPipeline.yml"));
        }

        _gateServiceMock.Setup(x => x.GetGatesForBuildDefinitionAsync(It.IsAny<Project>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Gate>());
    }

    private void InitializeProjectMocks(Project project)
    {
        _projectServiceMock.Setup(x => x.GetProjectByIdAsync(It.IsAny<string>(), project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _projectServiceMock.Setup(x => x.GetProjectByNameAsync(It.IsAny<string>(), project.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
    }

    private static void SetCustomTriggers(List<BuildTrigger>? customTriggers, BuildDefinition buildDefinition)
    {
        var field = typeof(BuildDefinition).GetField("m_triggers", BindingFlags.Instance | BindingFlags.NonPublic);
        field?.SetValue(buildDefinition, customTriggers);
    }

    private Dictionary<Project, IEnumerable<ReleaseDefinition>> InitializeReleaseDefinitionMocks(IReadOnlyList<int> pipelineIds, IReadOnlyCollection<Project> projects)
    {
        var returnDict = new Dictionary<Project, IEnumerable<ReleaseDefinition>>();
        var idCounter = 0;
        var projectCounter = 0;
        foreach (var project in projects)
        {
            var taskGroupId = _fixture.Create<Guid>();
            projectCounter++;
            var releaseDefinitions = new List<ReleaseDefinition>();
            if (idCounter > pipelineIds.Count)
            {
                break;
            }
            while (idCounter == 0 || idCounter / projects.Count != projectCounter)
            {
                if (idCounter >= pipelineIds.Count)
                {
                    break;
                }

                var releaseDefinition = new ReleaseDefinition
                {
                    Id = pipelineIds[idCounter],
                    Name = _fixture.Create<string>(),
                    Environments = new[]
                    {
                        new ReleaseDefinitionEnvironment
                        {
                            Id = _fixture.Create<int>(),
                            Name = _fixture.Create<string>(),
                            PreDeploymentGates = new ReleaseDefinitionGatesStep
                            {
                                GatesOptions = new ReleaseDefinitionGatesOptions
                                {
                                    IsEnabled = true
                                },
                                Gates = new []
                                {
                                    new ReleaseDefinitionGate
                                    {
                                        Tasks = new[]
                                        {
                                            new WorkflowTask
                                            {
                                                Inputs = new Dictionary<string, string>
                                                {
                                                    { "Function", _fixture.Create<string>() },
                                                    { "WaitForCompletion", "false" }
                                                }
                                            }
                                        }
                                    }
                                }
                            },
                            DeployPhases = new List<DeployPhase>
                            {
                                new AgentBasedDeployPhase
                                {
                                    Name = _fixture.Create<string>(),
                                    WorkflowTasks = new List<WorkflowTask>
                                    {
                                        new()
                                        {
                                            TaskId = _fixture.Create<Guid>(),
                                            Name = _fixture.Create<string>(),
                                            DefinitionType = "task",
                                            Inputs = new Dictionary<string, string>
                                            {
                                                { _fixture.Create<string>(), _fixture.Create<string>() },
                                                { _fixture.Create<string>(), _fixture.Create<string>() }
                                            }
                                        },
                                        new()
                                        {
                                            TaskId = taskGroupId,
                                            DefinitionType = "metaTask",
                                        }
                                    }
                                }
                            }
                        }
                    },
                    Artifacts = new[]
                    {
                        new Artifact
                        {
                            Type = "git",
                            DefinitionReference = new Dictionary<string, ArtifactSourceReference>
                            {
                                {
                                    "definition",
                                    new ArtifactSourceReference
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        Name = _fixture.Create<string>()
                                    }
                                }
                            }
                        }
                    }
                };

                idCounter++;
                releaseDefinitions.Add(releaseDefinition);

                _releaseRepositoryMock.Setup(x => x.GetReleaseDefinitionByIdAsync(project.Organization, project.Id, releaseDefinition.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(releaseDefinition);
            }

            var taskGroup = new TaskGroup
            {
                Id = taskGroupId,
                Tasks = new List<TaskGroupStep>
                {
                    new()
                    {
                        Task = new Microsoft.TeamFoundation.DistributedTask.WebApi.TaskDefinitionReference
                        {
                            Id = _fixture.Create<Guid>(),
                            DefinitionType = "task",
                        },
                        DisplayName = _fixture.Create<string>(),
                        Inputs = new Dictionary<string, string>
                        {
                            { _fixture.Create<string>(), _fixture.Create<string>() },
                            { _fixture.Create<string>(), _fixture.Create<string>() }
                        }
                    }
                }
            };

            returnDict.Add(project, releaseDefinitions);

            _releaseRepositoryMock.Setup(x => x.GetReleaseDefinitionsByProjectAsync(project.Organization, project.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(releaseDefinitions);
            _taskGroupRepositoryMock.Setup(x => x.GetTaskGroupsAsync(project.Organization, project.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { new TaskGroup { Id = _fixture.Create<Guid>() }, taskGroup });

        }
        return returnDict;
    }

    private IEnumerable<BuildDefinition> CreateDefaultBuildDefinitionList(int pipelineId)
    {
        var yamlProcess = _fixture.Create<YamlProcess>();

        return _fixture.Build<BuildDefinition>()
            .With(x => x.Id, pipelineId)
            .With(x => x.Process, yamlProcess)
            .Without(x => x.AuthoredBy)
            .CreateMany(1);
    }

    private static IMapper CreateMapper()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<PipelineToAzdoBuildDefinitionProfile>();
            cfg.AddProfile<PipelineToAzdoReleaseDefinitionProfile>();
        });

        return new Mapper(configuration);
    }
}