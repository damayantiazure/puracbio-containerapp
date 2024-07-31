using System;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using MemoryCache.Testing.Moq;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Rules.Tests.Rules;

public class ClassicReleasePipelineHasSm9ChangeTaskTests
{
    private const string Sm9CreateTask = "d0c045b6-d01d-4d69-882a-c21b18a35472";
    private const string Sm9ApproveTask = "73cb0c6a-0623-4814-8774-57dc1ef33858";
    private readonly Fixture _fixture = new Fixture { RepeatCount = 1 };
    private readonly IMemoryCache _cache = Create.MockedMemoryCache();

    public ClassicReleasePipelineHasSm9ChangeTaskTests()
    {
        _fixture.Customize(new AutoNSubstituteCustomization());
    }

    [Theory]
    [InlineData(Sm9CreateTask)]
    [InlineData(Sm9ApproveTask)]
    public async Task GivenPipeline_WhenEnabledSM9CreateOrApproveTask_ThenEvaluatesToTrue(Guid taskId)
    {
        //Arrange
        _fixture.Customize<WorkflowTask>(ctx => ctx
            .With(t => t.TaskId, taskId)
            .With(t => t.Enabled, true));
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var releasePipeline = _fixture.Create<ReleaseDefinition>();
        var client = Substitute.For<IAzdoRestClient>();

        //Act
        var rule = new ClassicReleasePipelineHasSm9ChangeTask(client, _cache);
        var result = await rule.EvaluateAsync(organization, projectId, releasePipeline);

        //Assert
        result.ShouldBe(true);
    }

    [Theory]
    [InlineData(Sm9CreateTask)]
    [InlineData(Sm9ApproveTask)]
    public async Task GivenPipeline_WhenDisabledSM9CreateOrApproveTask_ThenEvaluatesToFalse(Guid taskId)
    {
        //Arrange
        _fixture.Customize<WorkflowTask>(ctx => ctx
            .With(t => t.TaskId, taskId)
            .With(t => t.Enabled, false));
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var releasePipeline = _fixture.Create<ReleaseDefinition>();
        var client = Substitute.For<IAzdoRestClient>();

        //Act
        var rule = new ClassicReleasePipelineHasSm9ChangeTask(client, _cache);
        var result = await rule.EvaluateAsync(organization, projectId, releasePipeline);

        //Assert
        result.ShouldBe(false);
    }

    [Fact]
    public async Task GivenPipeline_WhenNoSM9Task_ThenEvaluatesToFalse()
    {
        //Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var releasePipeline = _fixture.Create<ReleaseDefinition>();
        var client = Substitute.For<IAzdoRestClient>();

        //Act
        var rule = new ClassicReleasePipelineHasSm9ChangeTask(client, _cache);
        var result = await rule.EvaluateAsync(organization, projectId, releasePipeline);

        //Assert
        result.ShouldBe(false);
    }

    [Theory]
    [InlineData(Sm9CreateTask)]
    [InlineData(Sm9ApproveTask)]
    public async Task GivenPipeline_WhenEnabledTaskGroupWithSM9CreateOrApproveTask_ThenEvaluatesToTrue(string taskId)
    {
        //Arrange
        _fixture.Customize<WorkflowTask>(ctx => ctx
            .With(t => t.DefinitionType, "metaTask")
            .With(t => t.Enabled, true));
        _fixture.Customize<BuildStep>(ctx => ctx
            .With(s => s.Enabled, true));
        _fixture.Customize<BuildTask>(ctx => ctx
            .With(t => t.Id, taskId));

        var taskGroupResponse = _fixture.Create<TaskGroupResponse>();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var releasePipeline = _fixture.Create<ReleaseDefinition>();

        var client = Substitute.For<IAzdoRestClient>();
        client.GetAsync(Arg.Any<IAzdoRequest<TaskGroupResponse>>(), organization)
            .Returns(taskGroupResponse);

        //Act
        var rule = new ClassicReleasePipelineHasSm9ChangeTask(client, _cache);
        var result = await rule.EvaluateAsync(organization, projectId, releasePipeline);

        //Assert
        result.ShouldBe(true);
    }

    [Theory]
    [InlineData(Sm9CreateTask)]
    [InlineData(Sm9ApproveTask)]
    public async Task GivenPipeline_WhenDisabledTaskGroupWithSM9CreateOrApproveTask_ThenEvaluatesToFalse(string taskId)
    {
        //Arrange
        _fixture.Customize<WorkflowTask>(ctx => ctx
            .With(t => t.DefinitionType, "metaTask")
            .With(t => t.Enabled, false));
        _fixture.Customize<BuildStep>(ctx => ctx
            .With(s => s.Enabled, true));
        _fixture.Customize<BuildTask>(ctx => ctx
            .With(t => t.Id, taskId));

        var taskGroupResponse = _fixture.Create<TaskGroupResponse>();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var releasePipeline = _fixture.Create<ReleaseDefinition>();

        var client = Substitute.For<IAzdoRestClient>();
        client.GetAsync(Arg.Any<IAzdoRequest<TaskGroupResponse>>(), organization)
            .Returns(taskGroupResponse);

        //Act
        var rule = new ClassicReleasePipelineHasSm9ChangeTask(client, _cache);
        var result = await rule.EvaluateAsync(organization, projectId, releasePipeline);

        //Assert
        result.ShouldBe(false);
    }

    [Fact]
    public async Task GivenPipeline_WhenTaskGroupWithoutSM9Task_ThenEvaluatesToFalse()
    {
        //Arrange
        _fixture.Customize<WorkflowTask>(ctx => ctx
            .With(t => t.DefinitionType, "metaTask")
            .With(t => t.Enabled, true));
        _fixture.Customize<BuildStep>(ctx => ctx
            .With(s => s.Enabled, true));

        var taskGroupResponse = _fixture.Create<TaskGroupResponse>();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var releasePipeline = _fixture.Create<ReleaseDefinition>();

        var client = Substitute.For<IAzdoRestClient>();
        client.GetAsync(Arg.Any<IAzdoRequest<TaskGroupResponse>>(), organization)
            .Returns(taskGroupResponse);

        //Act
        var rule = new ClassicReleasePipelineHasSm9ChangeTask(client, _cache);
        var result = await rule.EvaluateAsync(organization, projectId, releasePipeline);

        //Assert
        result.ShouldBe(false);
    }
}