using AutoFixture;
using AutoFixture.AutoNSubstitute;
using MemoryCache.Testing.Moq;
using NSubstitute;
using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.PipelineResources.Model;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using System;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Rules.Tests.Rules;

public class BuildPipelineHasFortifyTaskTests
{
    private readonly IFixture _fixture = new Fixture { RepeatCount = 1 };
    private readonly IYamlHelper _yamlHelper;
    private readonly IAzdoRestClient _azdoRestClient;
    private readonly BuildPipelineHasFortifyTask _sut;
    private const string TaskName = "FortifySCA";

    public BuildPipelineHasFortifyTaskTests()
    {
        _fixture.Customize(new AutoNSubstituteCustomization());
        CustomizeFixture();

        _yamlHelper = Substitute.For<IYamlHelper>();
        _azdoRestClient = Substitute.For<IAzdoRestClient>();

        var memoryCache = Create.MockedMemoryCache();

        _sut = new BuildPipelineHasFortifyTask(_azdoRestClient, memoryCache, _yamlHelper);
    }

    [Fact]
    public async Task GivenPipeline_WhenNestedTaskGroupWithFortifyTask_ThenEvaluatesToTrue()
    {
        // Arrange
        _fixture.Customize<BuildProcess>(ctx => ctx
            .With(p => p.Type, 1));
        _fixture.Customize<BuildTask>(ctx => ctx
            .Without(t => t.DefinitionType)
            .With(t => t.Id, "818386e5-c8a5-46c3-822d-954b3c8fb130"));
        var taskGroupResponse = _fixture.Create<TaskGroupResponse>();

        var buildPipeline = _fixture.Create<BuildDefinition>();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();

        _azdoRestClient.GetAsync(Arg.Any<IAzdoRequest<TaskGroupResponse>>(), organization)
            .Returns(taskGroupResponse);

        // Act
        var actual = await _sut.EvaluateAsync(organization, projectId, buildPipeline).ConfigureAwait(false);

        // Assert
        actual.ShouldBe(true);
    }

    [Fact]
    public async Task GivenPipeline_WhenNestedTaskGroupWithCircularDependencyAndNoFortifyTask_ThenEvaluatesToFalse()
    {
        // Arrange
        _fixture.Customize<BuildProcess>(ctx => ctx
            .With(p => p.Type, 1));
        _fixture.Customize<BuildTask>(ctx => ctx
            .With(t => t.DefinitionType, "metaTask")
            .With(t => t.Id, "df6aa8e5-82dc-468c-a794-a7990523363d"));

        var taskGroupResponse = _fixture.Create<TaskGroupResponse>();

        var buildPipeline = _fixture.Create<BuildDefinition>();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();

        _azdoRestClient.GetAsync(Arg.Any<IAzdoRequest<TaskGroupResponse>>(), organization)
            .Returns(taskGroupResponse);

        // Act
        var actual = await _sut.EvaluateAsync(organization, projectId, buildPipeline).ConfigureAwait(false);

        // Assert
        actual.ShouldBe(false);
    }

    [Fact]
    public async Task GivenPipeline_WhenStepsYamlFileWithFortifyTask_ThenEvaluatesToTrue()
    {
        // Arrange
        var buildPipeline = _fixture.Create<BuildDefinition>();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();

        var pipelineTaskInputs = _fixture.Build<PipelineTaskInputs>()
            .With(x => x.Enabled, true)
            .With(x => x.FullTaskName, $"{TaskName}@5").CreateMany();

        _yamlHelper.GetPipelineTasksAsync(organization, projectId, buildPipeline)
            .Returns(pipelineTaskInputs);

        // Act
        var actual = await _sut.EvaluateAsync(organization, projectId, buildPipeline).ConfigureAwait(false);

        actual.ShouldBe(true);
    }

    [Fact]
    public async Task GivenPipeline_WhenJobsYamlFileWithFortifyTask_ThenEvaluatesToTrue()
    {
        // Arrange
        var buildPipeline = _fixture.Create<BuildDefinition>();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();

        var pipelineTaskInputs = _fixture.Build<PipelineTaskInputs>()
            .With(x => x.Enabled, true)
            .With(x => x.FullTaskName, TaskName).CreateMany();

        _yamlHelper.GetPipelineTasksAsync(organization, projectId, buildPipeline)
            .Returns(pipelineTaskInputs);
        // Act
        var actual = await _sut.EvaluateAsync(organization, projectId, buildPipeline).ConfigureAwait(false);

        // Assert
        actual.ShouldBe(true);
    }

    [Fact]
    public async Task GivenPipeline_WhenJobsYamlFileWithDisabledFortifyTask_ThenEvaluatesToInconclusive()
    {
        // Arrange
        var buildPipeline = _fixture.Create<BuildDefinition>();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();

        var pipelineTaskInputs = _fixture.Build<PipelineTaskInputs>()
            .With(x => x.Enabled, false)
            .With(x => x.FullTaskName, TaskName).CreateMany();

        _yamlHelper.GetPipelineTasksAsync(organization, projectId, buildPipeline)
            .Returns(pipelineTaskInputs);

        // Act
        var actual = await _sut.EvaluateAsync(organization, projectId, buildPipeline).ConfigureAwait(false);

        // Assert
        actual.ShouldBe(false);
    }

    [Theory]
    [InlineData("Fortify_SCA@5")]
    [InlineData("_FortifySCA@5")]
    [InlineData("FortifySCA_@5")]
    [InlineData("Fortify_SCA")]
    [InlineData("_FortifySCA")]
    [InlineData("FortifySCA_")]
    [InlineData(" ")]
    [InlineData("")]
    [InlineData(null)]
    public async Task GivenPipeline_WhenYamlFileWithoutFortifyTask_ThenEvaluatesToFalse(string fortifyTask)
    {
        // Arrange
        var buildPipeline = _fixture.Create<BuildDefinition>();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();

        var pipelineTaskInputs = _fixture.Build<PipelineTaskInputs>()
            .With(x => x.Enabled, true)
            .With(x => x.FullTaskName, fortifyTask).CreateMany();

        _yamlHelper.GetPipelineTasksAsync(organization, projectId, buildPipeline)
            .Returns(pipelineTaskInputs);

        // Act
        var result = await _sut.EvaluateAsync(organization, projectId, buildPipeline).ConfigureAwait(false);

        // Assert
        result.ShouldBe(false);
    }

    private void CustomizeFixture()
    {
        _fixture.Customize<BuildProcess>(ctx => ctx
            .With(p => p.Type, 2));
        _fixture.Customize<Project>(ctx => ctx
            .With(x => x.Name, "projectA"));
        _fixture.Customize<Repository>(ctx => ctx
            .With(r => r.Url, new Uri("https://projectA.nl")));
        _fixture.Customize<BuildDefinition>(ctx => ctx
            .Without(b => b.Yaml)
            .Without(b => b.YamlUsedInRun));
        _fixture.Customize<BuildStep>(ctx => ctx
            .With(t => t.Enabled, true));
        _fixture.Customize<TaskGroup>(ctx => ctx
            .With(t => t.Tasks, _fixture.CreateMany<BuildStep>()));
        _fixture.Customize<TaskGroupResponse>(ctx => ctx
            .With(t => t.Value, _fixture.CreateMany<TaskGroup>()));
    }
}