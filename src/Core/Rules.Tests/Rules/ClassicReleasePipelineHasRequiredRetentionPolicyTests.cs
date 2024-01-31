using System.Linq;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Rules.Tests.Rules;

public class ClassicReleasePipelineHasRequiredRetentionPolicyTests
{
    private const string PipelineId = "1";
    private readonly IFixture _fixture = new Fixture {RepeatCount = 1}.Customize(new AutoNSubstituteCustomization());
    private readonly IAzdoRestClient _client = Substitute.For<IAzdoRestClient>();
        
    [Fact]
    public async Task EvaluateShouldReturnTrueWhenPipelineHasRequiredRetentionPolicy()
    {
        //Arrange
        // ReSharper disable twice RedundantArgumentDefaultValue
        CustomizePolicySettings(_fixture, 450, true);
        SetupClient(_client, _fixture);
        var releasePipeline = _fixture.Create<ReleaseDefinition>();

        //Act
        var rule = new ClassicReleasePipelineHasRequiredRetentionPolicy(_client);
        var result = await rule.EvaluateAsync("", "", releasePipeline);

        //Assert
        result.ShouldBe(true);
    }

    [Fact]
    public async Task EvaluateShouldReturnFalseWhenAnyStageWithinPipelineDoesNotHaveRequiredRetentionPolicy()
    {
        //Arrange
        // ReSharper disable twice RedundantArgumentDefaultValue
        CustomizePolicySettings(_fixture, 450, true);
        SetupClient(_client, _fixture);
        var releasePipeline = _fixture.Create<ReleaseDefinition>();

        if (releasePipeline.Environments.Any())
        {
            releasePipeline.Environments.First().RetentionPolicy.DaysToKeep = 0;
        }

        //Act
        var rule = new ClassicReleasePipelineHasRequiredRetentionPolicy(_client);
        var result = await rule.EvaluateAsync("", "", releasePipeline);

        //Assert 
        Assert.False(result);
    }

    [Fact]
    public async Task EvaluateShouldReturnFalseWhenReleasesAreRetainedShorterThenRequired()
    {
        //Arrange
        // ReSharper disable once RedundantArgumentDefaultValue
        CustomizePolicySettings(_fixture, 5, true);
        SetupClient(_client, _fixture);
        var releasePipeline = _fixture.Create<ReleaseDefinition>();

        //Act
        var rule = new ClassicReleasePipelineHasRequiredRetentionPolicy(_client);
        var result = await rule.EvaluateAsync("", "", releasePipeline);

        //Assert
        result.ShouldBe(false);
    }

    [Fact]
    public async Task EvaluateShouldReturnFalseWhenRetainBuildsIsDisabled()
    {
        //Arrange
        CustomizePolicySettings(_fixture, 500, false);
        SetupClient(_client, _fixture);
        var releasePipeline = _fixture.Create<ReleaseDefinition>();

        //Act
        var rule = new ClassicReleasePipelineHasRequiredRetentionPolicy(_client);
        var result = await rule.EvaluateAsync("", "", releasePipeline);

        //Assert
        result.ShouldBe(false);
    }

    [Fact]
    public async Task GivenPolicySettingsAreNotCorrect_WhenReconcile_ThenSettingsArePut()
    {
        //Arrange
        CustomizePolicySettings(_fixture, 10, false);
        SetupClient(_client, _fixture);

        //Act
        var rule = new ClassicReleasePipelineHasRequiredRetentionPolicy(_client) as IReconcile;
        await rule.ReconcileAsync("", "", PipelineId);

        // Assert
        await _client
            .Received()
            .PutAsync(Arg.Any<IAzdoRequest<ReleaseSettings>>(), Arg.Any<ReleaseSettings>(), Arg.Any<string>());
    }

    [Fact]
    public async Task GivenPolicySettingsAreCorrect_WhenReconcile_ThenPipelineIsUpdatedAnyway()
    {
        //Arrange
        CustomizePolicySettings(_fixture);
        SetupClient(_client, _fixture);

        //Act
        var rule = new ClassicReleasePipelineHasRequiredRetentionPolicy(_client) as IReconcile;
        await rule.ReconcileAsync("", "", PipelineId);

        // Assert
        await _client
            .Received()
            .PutAsync(Arg.Any<IAzdoRequest<object>>(), Arg.Any<JObject>(), Arg.Any<string>());
    }

    private static void CustomizePolicySettings(IFixture fixture, int daysToKeep = 450,
        bool retainBuild = true)
    {
        fixture.Customize<RetentionPolicy>(ctx => ctx
            .With(r => r.DaysToKeep, daysToKeep)
            .With(r => r.RetainBuild, retainBuild));
    }

    private static void SetupClient(IAzdoRestClient client, IFixture fixture)
    {
        client
            .GetAsync(Arg.Any<IAzdoRequest<ReleaseDefinition>>(), Arg.Any<string>())
            .Returns(fixture.Create<ReleaseDefinition>());

        client
            .GetAsync(Arg.Any<IAzdoRequest<ReleaseSettings>>(), Arg.Any<string>())
            .Returns(fixture.Create<ReleaseSettings>());


        client
            .GetAsync(Arg.Any<IAzdoRequest<JObject>>(), Arg.Any<string>())
            .Returns(new JObject());
    }
}