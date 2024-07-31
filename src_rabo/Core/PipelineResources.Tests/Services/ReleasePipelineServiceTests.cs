using MemoryCache.Testing.Moq;
using Microsoft.Extensions.Caching.Memory;
using Rabobank.Compliancy.Core.PipelineResources.Services;
using Rabobank.Compliancy.Domain.Constants;
using Rabobank.Compliancy.Domain.RuleProfiles;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.PipelineResources.Tests.Services;

public class ReleasePipelineServiceTests
{
    private const string Organization = "TestOrganization";
    private const string Project = "TestProject";
    private const string DownloadPipelineArtifacts = "61f2a582-95ae-4948-b34d-a1b3c4f6a737";
    private const string DownloadBuildArtifacts = "a433f589-fce1-4460-9ee6-44a624aeb1fb";
    private const string OtherTask = "2e91ea89-87cb-482e-9620-51bf6421e065";
    private readonly IFixture _fixture = new Fixture() { RepeatCount = 1 };
    private readonly IMemoryCache _cache = Create.MockedMemoryCache();

    private readonly Mock<IAzdoRestClient> _azdoRestClientMock = new();
    private readonly Mock<IBuildPipelineService> _buildPipelineServiceMock = new();
    private readonly ReleasePipelineService _sut;

    public ReleasePipelineServiceTests()
    {
        _sut = new ReleasePipelineService(_azdoRestClientMock.Object, _buildPipelineServiceMock.Object, _cache);
    }

    #region [build artifacts]

    [Fact]
    public async Task ShouldReturnLinkedPipelinesFromBuildArtifacts()
    {
        // Arrange
        _fixture.Customize<Artifact>(a => a
            .With(a => a.Type, "Build"));
        var input = _fixture.Create<ReleaseDefinition>();
        var output = CreateOutput();

        _azdoRestClientMock
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<BuildDefinition>>(), It.IsAny<string>()))
            .ReturnsAsync(output);

        _buildPipelineServiceMock
            .Setup(s => s.GetLinkedPipelinesAsync(Organization, It.IsAny<IEnumerable<BuildDefinition>>(), null))
            .ReturnsAsync(new[] { output });

        // Act
        var result = await _sut.GetLinkedPipelinesAsync(Organization, input, Project);

        // Assert
        _azdoRestClientMock.Verify(c => c.GetAsync(It.IsAny<IAzdoRequest<BuildDefinition>>(), It.IsAny<string>()), Times.Once);
        result.Should().HaveCount(1);
    }

    #endregion

    #region [MainframeCobol build task]

    [Fact]
    public async Task GetLinkedPipelinesAsync_WithCobolMainframeProfile_ShouldReturnLinkedPipelines()
    {
        // Arrange
        var dbbDeployTaskId = new Guid(TaskContants.MainframeCobolConstants.DbbDeployTaskId);
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>().ToString();

        var refOrganization = _fixture.Create<string>();
        var refProjectId = _fixture.Create<Guid>().ToString();
        var refPipelineId = _fixture.Create<Guid>().ToString();
        var refBuildDefinitions = _fixture.Create<BuildDefinition>();

        _fixture.Customize<WorkflowTask>(x => x
            .With(x => x.TaskId, dbbDeployTaskId)
            .With(x => x.Enabled, true)
            .With(x => x.Inputs, new Dictionary<string, string>
            {
                { TaskContants.MainframeCobolConstants.OrganizationName, refOrganization},
                { TaskContants.MainframeCobolConstants.ProjectId, refProjectId},
                { TaskContants.MainframeCobolConstants.PipelineId, refPipelineId}
            }));

        _fixture.Customize<PipelineRegistration>(x => x
            .With(x => x.RuleProfileName, nameof(Profiles.MainframeCobol)));

        _azdoRestClientMock.Setup(x =>
                x.GetAsync(It.IsAny<IAzdoRequest<BuildDefinition>>(), refOrganization))
            .ReturnsAsync(refBuildDefinitions);

        var buildDefinitions = _fixture.CreateMany<BuildDefinition>();
        var releaseDefinition = _fixture.Create<ReleaseDefinition>();
        _buildPipelineServiceMock.Setup(x => x.GetLinkedPipelinesAsync(organization, It.IsAny<IEnumerable<BuildDefinition>>(), buildDefinitions))
            .ReturnsAsync(new[] { refBuildDefinitions });

        // Act
        var actual = await _sut.GetLinkedPipelinesAsync(organization, releaseDefinition, projectId, buildDefinitions);

        // Assert        
        actual.Should().AllSatisfy(x => x.Should().Be(refBuildDefinitions));
    }

    #endregion

    #region [download artifacts task]

    [Theory]
    [InlineData(DownloadPipelineArtifacts, "source", "pipeline")]
    [InlineData(DownloadBuildArtifacts, "buildType", "definition")]
    public async Task PipelineWithEnabledDownloadTaskShouldReturnBuildDefinition(Guid taskId, string key, string key2)
    {
        //Arrange
        _fixture.Customize<WorkflowTask>(ctx => ctx
            .With(t => t.TaskId, taskId)
            .With(t => t.Enabled, true)
            .With(t => t.Inputs, new Dictionary<string, string> { [key] = "specific", ["project"] = "1", [key2] = "1" }));

        var input = _fixture.Create<ReleaseDefinition>();

        var output = CreateOutput();

        _azdoRestClientMock
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<BuildDefinition>>(), Organization))
            .ReturnsAsync(output);

        _buildPipelineServiceMock
            .Setup(s => s.GetLinkedPipelinesAsync(Organization, It.IsAny<IEnumerable<BuildDefinition>>(), null))
            .ReturnsAsync(new[] { output });

        //Act
        var result = await _sut.GetLinkedPipelinesAsync(Organization, input, Project);

        //Assert
        _azdoRestClientMock.Verify(c => c.GetAsync(It.IsAny<IAzdoRequest<BuildDefinition>>(), It.IsAny<string>()), Times.Once);
        result.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(OtherTask, true, "buildType", "specific", "definition")]
    [InlineData(DownloadBuildArtifacts, false, "buildType", "specific", "definition")]
    [InlineData(DownloadPipelineArtifacts, false, "source", "specific", "pipeline")]
    [InlineData(DownloadBuildArtifacts, true, "buildType", "current", "definition")]
    [InlineData(DownloadPipelineArtifacts, true, "source", "current", "pipeline")]
    public async Task PipelineWithout_OrWithInvalid_DownloadTaskShouldReturnNull(Guid taskId, bool enabled, string key, string value, string key2)
    {
        //Arrange
        _fixture.Customize<WorkflowTask>(ctx => ctx
            .With(t => t.TaskId, taskId)
            .With(t => t.Enabled, enabled)
            .With(t => t.Inputs, new Dictionary<string, string> { [key] = value, ["project"] = "1", [key2] = "1" }));

        var input = _fixture.Create<ReleaseDefinition>();

        _buildPipelineServiceMock
            .Setup(s => s.GetLinkedPipelinesAsync(Organization, It.IsAny<List<BuildDefinition>>(), null))
            .ReturnsAsync(_fixture.CreateMany<BuildDefinition>(0).ToList());

        //Act
        var result = await _sut.GetLinkedPipelinesAsync(Organization, input, Project);

        //Assert
        _azdoRestClientMock.Verify(c => c.GetAsync(It.IsAny<IAzdoRequest<BuildDefinition>>(), It.IsAny<string>()), Times.Never);
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(DownloadPipelineArtifacts, "source", "pipeline")]
    [InlineData(DownloadBuildArtifacts, "buildType", "definition")]
    public async Task PipelineWithEnabledTaskGroup_WithValidDownloadTask_ShoulReturnBuildDefinition(string taskId, string key, string key2)
    {
        //Arrange
        _fixture.Customize<WorkflowTask>(ctx => ctx
            .With(t => t.DefinitionType, "metaTask")
            .With(t => t.Enabled, true));
        _fixture.Customize<BuildStep>(ctx => ctx
            .With(s => s.Enabled, true)
            .With(t => t.Inputs, new Dictionary<string, string> { [key] = "specific", ["project"] = "1", [key2] = "1" }));
        _fixture.Customize<BuildTask>(ctx => ctx
            .With(t => t.Id, taskId));

        var input = _fixture.Create<ReleaseDefinition>();
        var output = CreateOutput();

        _azdoRestClientMock
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<TaskGroupResponse>>(), Organization))
            .ReturnsAsync(_fixture.Create<TaskGroupResponse>());
        _azdoRestClientMock
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<BuildDefinition>>(), Organization))
            .ReturnsAsync(output);

        _buildPipelineServiceMock
            .Setup(s => s.GetLinkedPipelinesAsync(Organization, It.IsAny<IEnumerable<BuildDefinition>>(), null))
            .ReturnsAsync(new[] { output });

        //Act
        var result = await _sut.GetLinkedPipelinesAsync(Organization, input, Project);

        //Assert
        _azdoRestClientMock.Verify(c => c.GetAsync(It.IsAny<IAzdoRequest<BuildDefinition>>(), It.IsAny<string>()), Times.Once);
        result.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(DownloadBuildArtifacts, false, "buildType", "specific", "definition")]
    [InlineData(DownloadPipelineArtifacts, false, "source", "specific", "pipeline")]
    [InlineData(OtherTask, true, "source", "specific", "pipeline")]
    [InlineData(DownloadBuildArtifacts, true, "buildType", "current", "definition")]
    [InlineData(DownloadPipelineArtifacts, true, "source", "current", "pipeline")]
    public async Task PipelineWithDisabledTaskGroup_OrTaskgroupWithInvalidDownloadTask_ShoulReturnNull(string taskId, bool enabled, string key, string value, string key2)
    {
        //Arrange
        _fixture.Customize<WorkflowTask>(ctx => ctx
            .With(t => t.DefinitionType, "metaTask")
            .With(t => t.Enabled, enabled));
        _fixture.Customize<BuildStep>(ctx => ctx
            .With(s => s.Enabled, true)
            .With(t => t.Inputs, new Dictionary<string, string> { [key] = value, ["project"] = "1", [key2] = "1" }));
        _fixture.Customize<BuildTask>(ctx => ctx
            .With(t => t.Id, taskId));

        var input = _fixture.Create<ReleaseDefinition>();

        _azdoRestClientMock
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<TaskGroupResponse>>(), Organization))
            .ReturnsAsync(_fixture.Create<TaskGroupResponse>());

        _buildPipelineServiceMock
            .Setup(s => s.GetLinkedPipelinesAsync(Organization, It.IsAny<List<BuildDefinition>>(), null))
            .ReturnsAsync(_fixture.CreateMany<BuildDefinition>(0).ToList());

        //Act
        var result = await _sut.GetLinkedPipelinesAsync(Organization, input, Project);

        // Assert
        _azdoRestClientMock.Verify(c => c.GetAsync(It.IsAny<IAzdoRequest<BuildDefinition>>(), It.IsAny<string>()), Times.Never);
        result.Should().BeEmpty();
    }

    #endregion

    #region [repo artifacts]

    [Fact]
    public async Task ShouldReturnLinkedRepositoriesFromRepoArtifactsAsync()
    {
        // Arrange
        _fixture.Customize<Artifact>(a => a
            .With(a => a.Type, "git"));
        var input = _fixture.CreateMany<ReleaseDefinition>(1).ToList();

        var buildPipelines = _fixture.CreateMany<BuildDefinition>(0).ToList();

        _buildPipelineServiceMock
            .Setup(s => s.GetLinkedRepositoriesAsync(Organization, It.IsAny<List<BuildDefinition>>()))
            .ReturnsAsync(_fixture.CreateMany<Repository>(0).ToList());

        // Act
        var result = await _sut.GetLinkedRepositoriesAsync(Organization, input, buildPipelines);

        // Assert
        result.Should().HaveCount(1);
    }

    #endregion

    [Fact]
    public async Task ShouldNotReportBuildpipelinesOrReposForReleaseWithoutResources()
    {
        // Arrange
        var artifacts = _fixture.Build<Artifact>()
            .CreateMany(0)
            .ToList();
        var input = _fixture.Build<ReleaseDefinition>()
            .With(r => r.Artifacts, artifacts)
            .Create();

        var buildPipelines = _fixture.CreateMany<BuildDefinition>(0).ToList();
        _buildPipelineServiceMock
            .Setup(s => s.GetLinkedPipelinesAsync(Organization, It.IsAny<List<BuildDefinition>>(), null))
            .ReturnsAsync(buildPipelines);
        _buildPipelineServiceMock
            .Setup(s => s.GetLinkedRepositoriesAsync(Organization, It.IsAny<List<BuildDefinition>>()))
            .ReturnsAsync(_fixture.CreateMany<Repository>(0).ToList());

        // Act
        var builds = await _sut.GetLinkedPipelinesAsync(Organization, input, Project);
        var repos = await _sut.GetLinkedRepositoriesAsync(Organization, new List<ReleaseDefinition> { input }, buildPipelines);

        // Assert
        _azdoRestClientMock.Verify(c => c.GetAsync(It.IsAny<IAzdoRequest<BuildDefinition>>(), It.IsAny<string>()), Times.Never);
        builds.Should().BeEmpty();
        repos.Should().BeEmpty();
    }

    private BuildDefinition CreateOutput()
    {
        return _fixture.Build<BuildDefinition>()
            .With(b => b.Id, "1")
            .Create();
    }
}