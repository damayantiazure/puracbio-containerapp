using AutoFixture;
using AutoFixture.AutoNSubstitute;
using MemoryCache.Testing.Moq;
using NSubstitute;
using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.PipelineResources.Model;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using Project = Rabobank.Compliancy.Infra.AzdoClient.Response.Project;
using Repository = Rabobank.Compliancy.Infra.AzdoClient.Response.Repository;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Rules.Tests.Rules;

public class BuildArtifactIsStoredSecureTests
{
    private readonly IFixture _fixture = new Fixture() { RepeatCount = 1 };
    private readonly IYamlHelper _yamlHelper;
    private readonly IAzdoRestClient _azdoRestClient;
    private readonly BuildArtifactIsStoredSecure _sut;

    public BuildArtifactIsStoredSecureTests()
    {
        _fixture.Customize(new AutoNSubstituteCustomization());
        CustomizeFixture();

        var memoryCache = Create.MockedMemoryCache();
        _yamlHelper = Substitute.For<IYamlHelper>();
        _azdoRestClient = Substitute.For<IAzdoRestClient>();

        _sut = new BuildArtifactIsStoredSecure(_azdoRestClient, memoryCache, _yamlHelper);
    }

    [Fact]
    public async Task GivenPipeline_WhenYamlWithPublish_ThenEvaluatesToTrue()
    {
        // Arrange
        var buildPipeline = _fixture.Create<BuildDefinition>();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();

        var pipelineInputTasks = _fixture.Build<PipelineTaskInputs>()
            .With(x => x.Enabled, true)
            .With(x => x.FullTaskName, "ecdc45f6-832d-4ad9-b52b-ee49e94659be@1")
            .CreateMany();

        _yamlHelper.GetPipelineTasksAsync(organization, projectId, buildPipeline)
            .Returns(pipelineInputTasks);

        // Act
        var actual = await _sut.EvaluateAsync(organization, projectId, buildPipeline);

        // Assert
        actual.ShouldBe(true);
    }

    [Fact]
    public async Task GivenPipeline_WhenYamlFileWithJobsAndPublish_ThenEvaluatesToTrue()
    {
        // Arrange
        var buildPipeline = _fixture.Create<BuildDefinition>();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipelineInputTasks = _fixture.Build<PipelineTaskInputs>()
            .With(x => x.Enabled, true)
            .With(x => x.FullTaskName, "PublishBuildArtifacts@2")
            .CreateMany();

        _yamlHelper.GetPipelineTasksAsync(organization, projectId, buildPipeline)
            .Returns(pipelineInputTasks);

        // Act
        var actual = await _sut.EvaluateAsync(organization, projectId, buildPipeline);

        // Assert
        actual.ShouldBe(true);
    }

    [Fact]
    public async Task EvaluateAsync_WhenYamlFileWithStagesAndPublish_ShouldReturnTrue()
    {
        // Arrange
        var buildPipeline = _fixture.Create<BuildDefinition>();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipelineInputTasks = _fixture.Build<PipelineTaskInputs>()
            .With(x => x.Enabled, true)
            .With(x => x.FullTaskName, "PublishBuildArtifacts@2")
            .CreateMany();

        _yamlHelper.GetPipelineTasksAsync(organization, projectId, buildPipeline)
            .Returns(pipelineInputTasks);
        // Act
        var actual = await _sut.EvaluateAsync(organization, projectId, buildPipeline);

        // Assert
        actual.ShouldBe(true);
    }

    [Fact]
    public async Task GivenPipeline_WhenYamlFileWithoutContent_ThenEvaluatesToFalse()
    {
        // Arrange
        var buildPipeline = _fixture.Create<BuildDefinition>();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();

        _yamlHelper.GetPipelineTasksAsync(organization, projectId, buildPipeline)
            .Returns(Enumerable.Empty<PipelineTaskInputs>());

        // Act
        var actual = await _sut.EvaluateAsync(organization, projectId, buildPipeline);

        // Assert
        actual.ShouldBe(false);
    }

    [Fact]
    public async Task EvaluateAsync_WithCorruptYamlFile_ShouldReturnFalse()
    {
        // Arrange
        var buildPipeline = _fixture.Create<BuildDefinition>();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();

        var yamlResponse = new YamlPipelineResponse { FinalYaml = "invalid" };
        _azdoRestClient.PostAsync(Arg.Any<IAzdoRequest<YamlPipeline.YamlPipelineRequest, YamlPipelineResponse>>(),
                Arg.Any<YamlPipeline.YamlPipelineRequest>(), organization, true)
            .Returns(yamlResponse);

        _yamlHelper.GetPipelineTasksAsync(organization, projectId, buildPipeline)
            .Returns(Enumerable.Empty<PipelineTaskInputs>());

        // Act
        var actual = await _sut.EvaluateAsync(organization, projectId, buildPipeline);

        // Assert
        actual.ShouldBe(false);
    }

    [Fact]
    public async Task GivenPipeline_WhenYamlFileWithDisabledPublish_ThenEvaluatesToFalse()
    {
        // Arrange
        var buildPipeline = _fixture.Create<BuildDefinition>();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipelineInputTasks = _fixture.Build<PipelineTaskInputs>()
            .With(x => x.Enabled, false)
            .With(x => x.FullTaskName, "PublishBuildArtifacts@1")
            .CreateMany();
        _yamlHelper.GetPipelineTasksAsync(organization, projectId, buildPipeline)
            .Returns(pipelineInputTasks);

        // Act
        var actual = await _sut.EvaluateAsync(organization, projectId, buildPipeline);

        // Assert
        actual.ShouldBe(false);
    }

    [Fact]
    public async Task GivenPipeline_WhenGuiAndNoPhases_ThenEvaluatesToException()
    {
        // Arrange
        _fixture.Customize<BuildProcess>(ctx => ctx
            .With(p => p.Type, 1)
            .Without(p => p.Phases));
        var buildPipeline = _fixture.Create<BuildDefinition>();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();

        _yamlHelper.GetPipelineTasksAsync(organization, projectId, buildPipeline)
            .Returns(new List<PipelineTaskInputs>());

        // Act
        var exception = await Record.ExceptionAsync(async () =>
            await _sut.EvaluateAsync(organization, projectId, buildPipeline));

        // Assert
        exception.ShouldNotBeNull();
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
    }
}