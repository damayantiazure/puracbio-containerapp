using AutoFixture;
using AutoFixture.AutoNSubstitute;
using MemoryCache.Testing.Moq;
using Moq;
using NSubstitute;
using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.PipelineResources.Model;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using System;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Rules.Tests.Rules;

public class BuildPipelineHasSonarqubeTaskTests
{
    private const string SonarTask = "6d01813a-9589-4b15-8491-8164aeb38055";
    private const string MavenTask = "ac4ee482-65da-4485-a532-7b085873e532";
    private const string OtherTask = "otherTask";
    private readonly Mock<IYamlHelper> _yamlHelper = new();

    private readonly Fixture _fixture = new() { RepeatCount = 1 };

    public BuildPipelineHasSonarqubeTaskTests() =>
        _fixture.Customize(new AutoNSubstituteCustomization());

    [Theory]
    [InlineData(SonarTask, "true", false, false)]
    [InlineData(SonarTask, "false", false, false)]
    [InlineData(SonarTask, "true", true, true)]
    [InlineData(SonarTask, "false", true, true)]
    [InlineData(MavenTask, "true", false, false)]
    [InlineData(MavenTask, "false", false, false)]
    [InlineData(MavenTask, "true", true, true)]
    [InlineData(MavenTask, "false", true, false)]
    [InlineData(OtherTask, "true", false, false)]
    [InlineData(OtherTask, "false", false, false)]
    [InlineData(OtherTask, "true", true, false)]
    [InlineData(OtherTask, "false", true, false)]
    public async Task GivenGuiPipeline_TaskNameAndInputAreVerifiedCorrectly(
        string task, string input, bool enabled, bool expectedResult)
    {
        //Assert
        _fixture.Customize<BuildProcess>(ctx => ctx
            .With(p => p.Type, 1));
        _fixture.Customize<BuildPhase>(ctx => ctx
            .With(bp => bp.Steps, new List<BuildStep>
            {
                new BuildStep
                {
                    Enabled = enabled,
                    Task = new BuildTask {Id = task},
                    Inputs = new Dictionary<string, string> {{ "sqAnalysisEnabled", input}}
                }
            }));
        _fixture.Customize<BuildDefinition>(ctx => ctx
            .Without(b => b.Yaml));

        var buildPipeline = _fixture.Create<BuildDefinition>();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var client = Substitute.For<IAzdoRestClient>();
        var cache = Create.MockedMemoryCache();

        //Act
        var rule = new BuildPipelineHasSonarqubeTask(client, cache, _yamlHelper.Object);
        var result = await rule.EvaluateAsync(organization, projectId, buildPipeline);

        //Assert
        result.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData("Maven@3", "true", true)]
    [InlineData("Maven@3", "false", false)]
    [InlineData("SonarQubeAnalyze@3", "true", true)]
    [InlineData("SonarQubeAnalyze@3", "false", true)]
    [InlineData("OtherTask@3", "true", false)]
    [InlineData("OtherTask@3", "false", false)]
    public async Task GivenYamlPipeline_TaskNameAndInputAreVerifiedCorrectly(
        string task, string input, bool expectedResult)
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
            { "sonarQubeRunAnalysis", $"{input}" }
        };

        var yamlHelper = Substitute.For<IYamlHelper>();
        yamlHelper.GetPipelineTasksAsync(organization, projectId, buildPipeline)
            .Returns(new List<PipelineTaskInputs>
            {
                new PipelineTaskInputs
                {
                    Enabled = true,
                    FullTaskName = $"{task}",
                    Inputs = pipelineInput
                }
            });

        var cache = Create.MockedMemoryCache();

        var rule = new BuildPipelineHasSonarqubeTask(null, cache, yamlHelper);
        var result = await rule.EvaluateAsync(organization, projectId, buildPipeline);

        result.ShouldBe(expectedResult);
    }
}