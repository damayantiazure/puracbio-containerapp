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

public class BuildPipelineHasNexusIqTaskTests
{
    private readonly Fixture _fixture = new Fixture { RepeatCount = 1 };
    private readonly Mock<IYamlHelper> _yamlHelper = new Mock<IYamlHelper>();

    public BuildPipelineHasNexusIqTaskTests()
    {
        _fixture.Customize(new AutoNSubstituteCustomization());
    }

    [Theory]
    [InlineData("4f40d1a2-83b0-4ddc-9a77-e7f279eb1802", true)]
    [InlineData("_4f40d1a2-83b0-4ddc-9a77-e7f279eb1802", false)]
    [InlineData("4f40d1a2-83b0-4ddc-9a77-e7f279eb1802_", false)]
    [InlineData(" 4f40d1a2-83b0-4ddc-9a77-e7f279eb1802", false)]
    [InlineData("4f40d1a2-83b0-4ddc-9a77-e7f279eb1802 ", false)]
    [InlineData("SomeThingWeird", false)]
    [InlineData(" ", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public async Task GivenGuiBuildPipeline_WhenNexusIqTask_ThenEvaluatesToExpectedResult(string taskId,
        bool expectedResult)
    {
        _fixture.Customize<BuildProcess>(ctx => ctx
            .With(p => p.Type, 1));
        _fixture.Customize<BuildStep>(ctx => ctx
            .With(s => s.Enabled, true));
        _fixture.Customize<BuildTask>(ctx => ctx
            .With(t => t.Id, taskId));
        _fixture.Customize<BuildDefinition>(ctx => ctx
            .Without(b => b.Yaml));

        var buildPipeline = _fixture.Create<BuildDefinition>();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var client = Substitute.For<IAzdoRestClient>();
        var cache = Create.MockedMemoryCache();

        //Act
        var rule = new BuildPipelineHasNexusIqTask(client, cache, _yamlHelper.Object);
        var result = await rule.EvaluateAsync(organization, projectId, buildPipeline);

        //Assert
        result.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData("SonatypeIntegrations.nexus-iq-azure-extension.nexus-iq-azure-pipeline-task.NexusIqPipelineTask@1",
        true)]
    [InlineData("SonatypeIntegrations.nexus-iq-azure-extension.nexus-iq-azure-pipeline-task.NexusIqPipelineTask@2",
        true)]
    [InlineData("SonatypeIntegrations.nexus-iq-azure-extension.nexus-iq-azure-pipeline-task.NexusIqPipelineTask",
        true)]
    [InlineData("SonatypeIntegrations.nexus-iq-azure-extension.nexus-iq-azure-pipeline-task.NexusIqPipelineTask_@1",
        false)]
    [InlineData("SonatypeIntegrations.nexus-iq-azure-extension.nexus-iq-azure-pipeline-task._NexusIqPipelineTask@2",
        false)]
    [InlineData("SonatypeIntegrations.nexus-iq-azure-extension.nexus-iq-azure-pipeline-task.NexusIqPipelineTask_",
        false)]
    [InlineData("SonatypeIntegrations.nexus-iq-azure-extension.nexus-iq-azure-pipeline-task._NexusIqPipelineTask",
        false)]
    [InlineData("SomeThingWeird", false)]
    [InlineData(" ", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public async Task GivenYamlBuildPipeline_WhenNexusIqTask_ThenEvaluatesToExpectedResult(string taskName,
        bool expectedResult)
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

        var yamlHelper = Substitute.For<IYamlHelper>();
        yamlHelper.GetPipelineTasksAsync(organization, projectId, buildPipeline)
            .Returns(new List<PipelineTaskInputs>
            {
                new PipelineTaskInputs
                {
                    Enabled = true,
                    FullTaskName = $"{taskName}"
                }
            });

        var cache = Create.MockedMemoryCache();

        var rule = new BuildPipelineHasNexusIqTask(null, cache, yamlHelper);
        var result = await rule.EvaluateAsync(organization, projectId, buildPipeline);

        result.ShouldBe(expectedResult);
    }
}