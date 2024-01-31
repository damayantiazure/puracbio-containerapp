using AutoFixture;
using AutoFixture.AutoNSubstitute;
using MemoryCache.Testing.Moq;
using NSubstitute;
using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.PipelineResources.Model;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using System;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Rules.Tests.Rules;

public class BuildPipelineHasCredScanTaskTests
{
    private readonly Fixture _fixture = new Fixture { RepeatCount = 1 };

    public BuildPipelineHasCredScanTaskTests()
    {
        _fixture.Customize(new AutoNSubstituteCustomization());
    }

    [Fact]
    public void BuildPipelineHasCredScanTask_ShouldHaveCorrectProperties()
    {
        var rule = new BuildPipelineHasCredScanTask(null, null, null);

        // Assert
        Assert.Equal("Build pipeline contains CredScan task", ((IRule)rule).Description);
        Assert.Equal("https://confluence.dev.rabobank.nl/x/LorHDQ", ((IRule)rule).Link);
    }

    [Theory]
    [InlineData("f0462eae-4df1-45e9-a754-8184da95ed01", "dbe519ee-a2e4-43f5-8e1a-949bd935b736", true)]
    [InlineData("SomethingWrong", "dbe519ee-a2e4-43f5-8e1a-949bd935b736", false)]
    public async Task GivenGuiBuildPipeline_WhenCredScanTask_ThenEvaluatesToExpectedResult(string credScanTaskId,
        string postAnalysisTaskId,
        bool expectedResult)
    {
        //Assert
        _fixture.Customize<BuildProcess>(ctx => ctx
            .With(p => p.Type, 1));
        _fixture.Customize<BuildPhase>(ctx => ctx
            .With(bp => bp.Steps, new List<BuildStep>
            {
                new BuildStep {Enabled = true, Task = new BuildTask {Id = credScanTaskId}},
                new BuildStep
                {
                    Enabled = true, Task = new BuildTask {Id = postAnalysisTaskId},
                    Inputs = new Dictionary<string, string> {{"CredScan", "true"}}
                }
            }));

        var buildPipeline = _fixture.Create<BuildDefinition>();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var client = Substitute.For<IAzdoRestClient>();
        var cache = Create.MockedMemoryCache();
        var yamlHelper = Substitute.For<IYamlHelper>();

        //Act
        var rule = new BuildPipelineHasCredScanTask(client, cache, yamlHelper);
        var result = await rule.EvaluateAsync(organization, projectId, buildPipeline);

        //Assert
        result.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData("CredScan", "PostAnalysis")]
    [InlineData("CredScanOdd", "PostAnalysis")]
    public async Task GivenYamlBuildPipeline_WhenCredScanTask_ThenEvaluatesToExpectedResult(
        string credScanTaskName, string postAnalysisTaskName)
    {
        _fixture.Customize<BuildProcess>(ctx => ctx
            .With(p => p.Type, 2));
        _fixture.Customize<Project>(ctx => ctx
            .With(x => x.Name, "projectA"));
        _fixture.Customize<Repository>(ctx => ctx
            .With(r => r.Url, new Uri("https://projectA.nl")));
        _fixture.Customize<BuildDefinition>(ctx => ctx
            .Without(b => b.Yaml));

        var buildPipeline = _fixture.Create<BuildDefinition>();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();

        var yamlHelper = Substitute.For<IYamlHelper>();
        yamlHelper.GetPipelineTasksAsync(organization, projectId, buildPipeline)
            .Returns(new List<PipelineTaskInputs>
            {
                new PipelineTaskInputs
                {
                    Enabled = true,
                    FullTaskName = $"{credScanTaskName}"
                },
                new PipelineTaskInputs
                {
                    Enabled = true,
                    FullTaskName = $"{postAnalysisTaskName}",
                }
            });

        var cache = Create.MockedMemoryCache();
        var rule = new BuildPipelineHasCredScanTask(null, cache, yamlHelper);
        var result = await rule.EvaluateAsync(organization, projectId, buildPipeline);

        result.ShouldBe(false);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("123", false)]
    public async Task GivenYamlBuildPipeline_WhenCredScanTaskWithValidInput_ThenEvaluatesToTrue(
        string propertyValue, bool expectedResult)
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

        var buildPipeline = _fixture.Create<BuildDefinition>();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();

        var pipelineInput = new Dictionary<string, string>
        {
            { "CredScan", $"{propertyValue}" },
            { "ToolLogsNotFoundAction", "error"}
        };

        var yamlHelper = Substitute.For<IYamlHelper>();
        yamlHelper.GetPipelineTasksAsync(organization, projectId, buildPipeline)
            .Returns(new List<PipelineTaskInputs>
            {
                new PipelineTaskInputs
                {
                    Enabled = true,
                    FullTaskName = "CredScan"
                },
                new PipelineTaskInputs
                {
                    Enabled = true,
                    FullTaskName = "PostAnalysis",
                    Inputs = pipelineInput
                }
            });


        var cache = Create.MockedMemoryCache();
        var rule = new BuildPipelineHasCredScanTask(null, cache, yamlHelper);
        var result = await rule.EvaluateAsync(organization, projectId, buildPipeline);

        result.ShouldBe(expectedResult);
    }
}