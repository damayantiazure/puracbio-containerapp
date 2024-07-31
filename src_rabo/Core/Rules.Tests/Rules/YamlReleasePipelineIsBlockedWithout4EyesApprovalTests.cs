using AutoFixture;
using Moq;
using Newtonsoft.Json.Linq;
using Rabobank.Compliancy.Core.Rules.Helpers;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Core.Rules.Tests.Objects;
using Rabobank.Compliancy.Core.Rules.Tests.Resources;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.StorageClient;
using Shouldly;
using System.Linq;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Rules.Tests.Rules;

public class YamlReleasePipelineIsBlockedWithout4EyesApprovalTests
{
    private readonly Fixture _fixture = new();

    private readonly string _pipelineYamlDefault = PipelineYaml.SingleStageSingleEnvironment;

    private readonly string _environmentNameDefault = PipelineYaml.FirstEnvironment.Name;

    private readonly string _stageNameDefault = PipelineYaml.FirstProdStage;

    private readonly JObject _environmentChecksDefault = EnvironmentChecks.ValidateYamlApproversSingle;
    private readonly JObject _environmentWithMultiCheck = EnvironmentChecks.ValidateYamlApproversWithMultiChecks;

    private readonly RuleConfig _ruleConfigDefault = new() { ValidateGatesHostName = "https://validategatesdev.azurewebsites.net" };

    [Fact]
    public async Task EvaluateAsync_WithPipelineWithValidGate_ShouldReturnTrue()
    {
        // arrange
        _fixture.Customize<BuildDefinition>(ctx => ctx
            .Without(b => b.Yaml)
            .Without(b => b.YamlUsedInRun));

        var clientMock = CreateClientMock(_pipelineYamlDefault, _environmentChecksDefault, _environmentNameDefault);
        var resolverMock = CreateResolverMock(_stageNameDefault);
        var yamlEnvironmentHelper = new YamlEnvironmentHelper(clientMock.Object, resolverMock.Object);

        var sut = new YamlReleasePipelineIsBlockedWithout4EyesApproval(clientMock.Object, yamlEnvironmentHelper, _ruleConfigDefault);

        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipeline = _fixture.Create<BuildDefinition>();

        // act
        var actual = await sut.EvaluateAsync(organization, projectId, pipeline).ConfigureAwait(false);

        // assert
        actual.ShouldBe(true);
    }

    [Fact]
    public async Task EvaluateAsync_WithCompleteObjectAsParameter_ShouldPassCompleteObjectToYamlEnvironmentHelper()
    {
        // arrange
        var clientMock = CreateClientMock(_pipelineYamlDefault, _environmentChecksDefault, _environmentNameDefault);
        var yamlEnvironmentHelperMock = new Mock<IYamlEnvironmentHelper>();

        var sut = new YamlReleasePipelineIsBlockedWithout4EyesApproval(clientMock.Object, yamlEnvironmentHelperMock.Object, _ruleConfigDefault);

        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipeline = _fixture.Create<BuildDefinition>();

        // act
        var actual = await sut.EvaluateAsync(organization, projectId, pipeline).ConfigureAwait(false);

        // assert
        yamlEnvironmentHelperMock.Verify(mock => mock.GetProdEnvironmentsAsync(organization, projectId,
            It.Is<BuildDefinition>(x =>
                x.Id == pipeline.Id &&
                x.Name == pipeline.Name &&
                x.Process == pipeline.Process &&
                x.Project == pipeline.Project &&
                x.Repository == pipeline.Repository &&
                x.Path == pipeline.Path &&
                x.QueueStatus == pipeline.QueueStatus &&
                x.AuthoredBy == pipeline.AuthoredBy &&
                x.Triggers == pipeline.Triggers &&
                x.PipelineType == pipeline.PipelineType &&
                x.Yaml == pipeline.Yaml &&
                x.YamlUsedInRun == pipeline.YamlUsedInRun &&
                x.Stages == pipeline.Stages &&
                x.PipelineRegistrations == pipeline.PipelineRegistrations &&
                x.Links == pipeline.Links)
            ), Times.Once);
    }

    [Fact]
    public async Task EvaluateAsync_WithSingleStage_WithMultipleCompliantEnvironments_ShouldReturnTrue()
    {
        // arrange
        _fixture.Customize<BuildDefinition>(ctx => ctx
            .Without(b => b.Yaml)
            .Without(b => b.YamlUsedInRun));

        var environmentMock1 = new EnvironmentMock(PipelineYaml.FirstEnvironment, EnvironmentChecks.ValidateEnvironmentSingle);

        var environmentMock2 = new EnvironmentMock(PipelineYaml.SecondEnvironment, EnvironmentChecks.ValidateEnvironmentSingle);

        var environmentMocks = new[] { environmentMock1, environmentMock2 };

        var clientMock = CreateClientMock(PipelineYaml.SingleProdStagesWithTwoEnvironments, environmentMocks);
        var resolverMock = CreateResolverMock(_stageNameDefault);
        var yamlEnvironmentHelper = new YamlEnvironmentHelper(clientMock.Object, resolverMock.Object);

        var sut = new YamlReleasePipelineIsBlockedWithout4EyesApproval(clientMock.Object, yamlEnvironmentHelper, _ruleConfigDefault);

        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipeline = _fixture.Create<BuildDefinition>();

        // act
        var actual = await sut.EvaluateAsync(organization, projectId, pipeline).ConfigureAwait(false);

        // assert
        actual.ShouldBe(true);
    }

    [Fact]
    public async Task EvaluateAsync_WithSingleStage_WithIncompliantEnvironments_ShouldReturnFalse()
    {
        // arrange
        _fixture.Customize<BuildDefinition>(ctx => ctx
            .Without(b => b.Yaml)
            .Without(b => b.YamlUsedInRun));

        var environmentMock1 = new EnvironmentMock
        {
            EnvironmentYaml = PipelineYaml.FirstEnvironment,
            EnvironmentCheck = EnvironmentChecks.ValidateEnvironmentSingle
        };

        var environmentMock2 = new EnvironmentMock
        {
            EnvironmentYaml = PipelineYaml.SecondEnvironment,
            EnvironmentCheck = EnvironmentChecks.NoChecks
        };

        var environmentMocks = new[] { environmentMock1, environmentMock2 };

        var clientMock = CreateClientMock(PipelineYaml.SingleProdStagesWithTwoEnvironments, environmentMocks);
        var resolverMock = CreateResolverMock(_stageNameDefault);
        var yamlEnvironmentHelper = new YamlEnvironmentHelper(clientMock.Object, resolverMock.Object);

        var sut = new YamlReleasePipelineIsBlockedWithout4EyesApproval(clientMock.Object, yamlEnvironmentHelper, _ruleConfigDefault);

        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipeline = _fixture.Create<BuildDefinition>();

        // act
        var actual = await sut.EvaluateAsync(organization, projectId, pipeline).ConfigureAwait(false);

        // assert
        actual.ShouldBe(false);
    }

    [Fact]
    public async Task EvaluateAsync_WithMultipleStages_WithMultipleCompliantEnvironments_ShouldReturnTrue()
    {
        // arrange
        _fixture.Customize<BuildDefinition>(ctx => ctx
            .Without(b => b.Yaml)
            .Without(b => b.YamlUsedInRun));

        var environmentMock1 = new EnvironmentMock(PipelineYaml.FirstEnvironment, EnvironmentChecks.ValidateEnvironmentSingle);

        var environmentMock2 = new EnvironmentMock(PipelineYaml.SecondEnvironment, EnvironmentChecks.ValidateEnvironmentSingle);

        var environmentMocks = new[] { environmentMock1, environmentMock2 };

        var clientMock = CreateClientMock(PipelineYaml.TwoProdStagesWithDifferentEnvironment, environmentMocks);
        var resolverMock = CreateResolverMock(PipelineYaml.FirstProdStage, PipelineYaml.SecondProdStage);
        var yamlEnvironmentHelper = new YamlEnvironmentHelper(clientMock.Object, resolverMock.Object);

        var sut = new YamlReleasePipelineIsBlockedWithout4EyesApproval(clientMock.Object, yamlEnvironmentHelper, _ruleConfigDefault);

        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipeline = _fixture.Create<BuildDefinition>();

        // act
        var actual = await sut.EvaluateAsync(organization, projectId, pipeline).ConfigureAwait(false);

        // assert
        actual.ShouldBe(true);
    }

    [Fact]
    public async Task EvaluateAsync_WithEnvironmentWithMultipleChecks_ShouldReturnTrue()
    {
        // arrange

        _fixture.Customize<BuildDefinition>(ctx => ctx
            .Without(b => b.Yaml)
            .Without(b => b.YamlUsedInRun));

        var clientMock = CreateClientMock(_pipelineYamlDefault, EnvironmentChecks.ValidateYamlApproversWithOtherChecks, _environmentNameDefault);
        var resolverMock = CreateResolverMock(_stageNameDefault);
        var yamlEnvironmentHelper = new YamlEnvironmentHelper(clientMock.Object, resolverMock.Object);

        var sut = new YamlReleasePipelineIsBlockedWithout4EyesApproval(clientMock.Object, yamlEnvironmentHelper, _ruleConfigDefault);

        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipeline = _fixture.Create<BuildDefinition>();

        // act
        var actual = await sut.EvaluateAsync(organization, projectId, pipeline);

        // assert
        actual.ShouldBe(true);
    }

    [Fact]
    public async Task EvaluateAsync_WithMultipleRegisteredStagesButOnlyOneUsed_ShouldReturnTrue()
    {
        // arrange
        _fixture.Customize<BuildDefinition>(ctx => ctx
            .Without(b => b.Yaml)
            .Without(b => b.YamlUsedInRun));

        var clientMock = CreateClientMock(_pipelineYamlDefault, _environmentChecksDefault, _environmentNameDefault);
        var resolverMock = CreateResolverMock(_stageNameDefault, "SomeOtherStage");
        var yamlEnvironmentHelper = new YamlEnvironmentHelper(clientMock.Object, resolverMock.Object);

        var sut = new YamlReleasePipelineIsBlockedWithout4EyesApproval(clientMock.Object, yamlEnvironmentHelper, _ruleConfigDefault);

        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipeline = _fixture.Create<BuildDefinition>();

        // act
        var actual = await sut.EvaluateAsync(organization, projectId, pipeline);

        // assert
        actual.ShouldBe(true);
    }

    [Fact]
    public async Task EvaluateAsync_WithProductionStageWithoutEnvironment_ShouldReturnFalse()
    {
        // arrange

        _fixture.Customize<BuildDefinition>(ctx => ctx
            .Without(b => b.Yaml));

        var clientMock = CreateClientMock(PipelineYaml.TwoProdStagesFirstWithSecondWithoutEnvironment, _environmentChecksDefault, _environmentNameDefault);
        var resolverMock = CreateResolverMock(PipelineYaml.FirstProdStage, PipelineYaml.SecondProdStage);
        var yamlEnvironmentHelper = new YamlEnvironmentHelper(clientMock.Object, resolverMock.Object);

        var sut = new YamlReleasePipelineIsBlockedWithout4EyesApproval(clientMock.Object, yamlEnvironmentHelper, _ruleConfigDefault);

        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipeline = _fixture.Create<BuildDefinition>();

        // act
        var actual = await sut.EvaluateAsync(organization, projectId, pipeline);

        // assert
        actual.ShouldBe(false);
    }

    [Fact]
    public async Task EvaluateAsync_WithProductionStageWithSharedEnvironment_ShouldReturnTrue()
    {
        // arrange

        _fixture.Customize<BuildDefinition>(ctx => ctx
            .Without(b => b.Yaml)
            .Without(b => b.YamlUsedInRun));

        var clientMock = CreateClientMock(PipelineYaml.TwoProdStagesWithSameEnvironment, _environmentChecksDefault, _environmentNameDefault);
        var resolverMock = CreateResolverMock(PipelineYaml.FirstProdStage, PipelineYaml.SecondProdStage);
        var yamlEnvironmentHelper = new YamlEnvironmentHelper(clientMock.Object, resolverMock.Object);

        var sut = new YamlReleasePipelineIsBlockedWithout4EyesApproval(clientMock.Object, yamlEnvironmentHelper, _ruleConfigDefault);

        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipeline = _fixture.Create<BuildDefinition>();

        // act
        var actual = await sut.EvaluateAsync(organization, projectId, pipeline);

        // assert
        actual.ShouldBe(true);
    }

    [Fact]
    public async Task EvaluateAsync_WithPipelineWithValidGateWithAlternativeStageAndEnvironmentNames_ShouldReturnTrue()
    {
        // arrange

        _fixture.Customize<BuildDefinition>(ctx => ctx
            .Without(b => b.Yaml)
            .Without(b => b.YamlUsedInRun));

        var clientMock = CreateClientMock(PipelineYaml.SingleStageSingleEnvironmentWithAlternativeNames, _environmentChecksDefault, _environmentNameDefault);
        var resolverMock = CreateResolverMock(_stageNameDefault);
        var yamlEnvironmentHelper = new YamlEnvironmentHelper(clientMock.Object, resolverMock.Object);

        var sut = new YamlReleasePipelineIsBlockedWithout4EyesApproval(clientMock.Object, yamlEnvironmentHelper, _ruleConfigDefault);

        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipeline = _fixture.Create<BuildDefinition>();

        // act
        var actual = await sut.EvaluateAsync(organization, projectId, pipeline);

        // assert
        actual.ShouldBe(true);
    }

    [Fact]
    public async Task EvaluateAsync_WithPipelineWithInvalidGateUrl_ShouldReturnFalse()
    {
        // arrange

        _fixture.Customize<BuildDefinition>(ctx => ctx
            .Without(b => b.Yaml));

        var clientMock = CreateClientMock(_pipelineYamlDefault, EnvironmentChecks.InvalidValidateGatesUrl, _environmentNameDefault);
        var resolverMock = CreateResolverMock(_stageNameDefault);
        var yamlEnvironmentHelper = new YamlEnvironmentHelper(clientMock.Object, resolverMock.Object);

        var sut = new YamlReleasePipelineIsBlockedWithout4EyesApproval(clientMock.Object, yamlEnvironmentHelper, _ruleConfigDefault);

        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipeline = _fixture.Create<BuildDefinition>();

        // act
        var actual = await sut.EvaluateAsync(organization, projectId, pipeline);

        // assert
        actual.ShouldBe(false);
    }

    [Fact]
    public async Task EvaluateAsync_PipelineWithNewGate_WithoutCallBackSetting_ShouldReturnFalse()
    {
        // arrange
        _fixture.Customize<BuildDefinition>(ctx => ctx
            .Without(b => b.Yaml));

        var clientMock = CreateClientMock(_pipelineYamlDefault, EnvironmentChecks.WithoutCallBack, _environmentNameDefault);
        var resolverMock = CreateResolverMock(_stageNameDefault);
        var yamlEnvironmentHelper = new YamlEnvironmentHelper(clientMock.Object, resolverMock.Object);

        var sut = new YamlReleasePipelineIsBlockedWithout4EyesApproval(clientMock.Object, yamlEnvironmentHelper, _ruleConfigDefault);

        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipeline = _fixture.Create<BuildDefinition>();

        // act
        var actual = await sut.EvaluateAsync(organization, projectId, pipeline);

        // assert
        actual.ShouldBe(false);
    }

    [Fact]
    public async Task EvaluateAsync_WithNoProjectEnvironments_ShouldReturnFalse()
    {
        // arrange
        _fixture.Customize<BuildDefinition>(ctx => ctx
            .Without(b => b.Yaml));

        var clientMock = CreateClientMock(_pipelineYamlDefault, _environmentChecksDefault);
        var resolverMock = CreateResolverMock(_stageNameDefault);
        var yamlEnvironmentHelper = new YamlEnvironmentHelper(clientMock.Object, resolverMock.Object);

        var sut = new YamlReleasePipelineIsBlockedWithout4EyesApproval(clientMock.Object, yamlEnvironmentHelper, _ruleConfigDefault);

        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipeline = _fixture.Create<BuildDefinition>();

        // act
        var actual = await sut.EvaluateAsync(organization, projectId, pipeline);

        // assert
        actual.ShouldBe(false);
    }

    [Fact]
    public async Task ReconcileAsync_WithPipelineWithoutGate_GateShouldBeCreated()
    {
        // arrange

        var clientMock = CreateClientMock(_pipelineYamlDefault, EnvironmentChecks.NoChecks, _environmentNameDefault);
        var resolverMock = CreateResolverMock(_stageNameDefault);
        var yamlEnvironmentHelper = new YamlEnvironmentHelper(clientMock.Object, resolverMock.Object);

        var sut = new YamlReleasePipelineIsBlockedWithout4EyesApproval(clientMock.Object, yamlEnvironmentHelper, _ruleConfigDefault);

        // act
        await sut.ReconcileAsync(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());

        // assert
        clientMock.Verify(m => m.PostAsync(
            It.IsAny<IAzdoRequest<object, JObject>>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task ReconcileAsync_WithPipelineWithMultiCheck_AllCompliancyChecksShouldBeDeletedAndMakeNewOne()
    {
        var clientMock = CreateClientMock(_pipelineYamlDefault, _environmentWithMultiCheck, _environmentNameDefault);
        var resolverMock = CreateResolverMock(_stageNameDefault);
        var yamlEnvironmentHelper = new YamlEnvironmentHelper(clientMock.Object, resolverMock.Object);

        var sut = new YamlReleasePipelineIsBlockedWithout4EyesApproval(clientMock.Object, yamlEnvironmentHelper, _ruleConfigDefault);

        // act
        await sut.ReconcileAsync(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());

        // assert

        clientMock.Verify(m => m.DeleteAsync(
            It.IsAny<IAzdoRequest>(), It.IsAny<string>()), Times.Exactly(2));
        clientMock.Verify(m => m.PostAsync(
            It.IsAny<IAzdoRequest<object, JObject>>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
    }

    private static Mock<IPipelineRegistrationResolver> CreateResolverMock(params string[] stages)
    {
        var resolverMock = new Mock<IPipelineRegistrationResolver>();
        resolverMock.Setup(m => m.ResolveProductionStagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(stages);
        return resolverMock;
    }

    private Mock<IAzdoRestClient> CreateClientMock(string pipelineYaml, JObject environmentChecksJson,
        string projectEnvironmentName = null)
    {
        var response = _fixture.Create<YamlPipelineResponse>();
        response.FinalYaml = pipelineYaml;

        var clientMock = new Mock<IAzdoRestClient>();
        clientMock.Setup(m => m.PostAsync(
                It.IsAny<IAzdoRequest<YamlPipeline.YamlPipelineRequest, YamlPipelineResponse>>(),
                It.IsAny<YamlPipeline.YamlPipelineRequest>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(response);

        if (projectEnvironmentName != null)
        {
            var projectEnvironment = _fixture.Create<EnvironmentYaml>();
            projectEnvironment.Name = projectEnvironmentName;
            clientMock.Setup(m => m.GetAsync(It.IsAny<IEnumerableRequest<EnvironmentYaml>>(), It.IsAny<string>()))
                .ReturnsAsync(new[] { projectEnvironment });
        }

        clientMock.Setup(m => m.GetAsync(It.IsAny<IAzdoRequest<JObject>>(), It.IsAny<string>()))
            .ReturnsAsync(environmentChecksJson);
        return clientMock;
    }

    private Mock<IAzdoRestClient> CreateClientMock(string pipelineYaml, EnvironmentMock[] environmentMocks)
    {
        var response = _fixture.Create<YamlPipelineResponse>();
        response.FinalYaml = pipelineYaml;

        var clientMock = new Mock<IAzdoRestClient>();
        clientMock.Setup(m => m.PostAsync(
                It.IsAny<IAzdoRequest<YamlPipeline.YamlPipelineRequest, YamlPipelineResponse>>(),
                It.IsAny<YamlPipeline.YamlPipelineRequest>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(response);

        if (environmentMocks != null && environmentMocks.Any())
        {
            clientMock.Setup(m => m.GetAsync(It.IsAny<IEnumerableRequest<EnvironmentYaml>>(), It.IsAny<string>()))
                .ReturnsAsync(environmentMocks.Select(e => e.EnvironmentYaml));
        }

        foreach (var environmentMock in environmentMocks)
        {
            clientMock.Setup(m => m.GetAsync(It.Is<IAzdoRequest<JObject>>(r => r.Resource.Contains($"/_environments/{environmentMock.EnvironmentYaml.Id}/checks")), It.IsAny<string>()))
                .ReturnsAsync(environmentMock.EnvironmentCheck);
        }

        return clientMock;
    }
}