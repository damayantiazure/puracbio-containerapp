using Newtonsoft.Json.Linq;
using Rabobank.Compliancy.Core.PipelineResources.Extensions;
using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.PipelineResources.Model;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Collections.Generic;

namespace Rabobank.Compliancy.Core.PipelineResources.Tests.Helpers;

using FluentAssertions;
using System.Threading.Tasks;

public class YamlPipelineEvaluatorTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IAzdoRestClient> _azdoRestClientMock;
    private readonly Mock<IYamlHelper> _yamlHelperMock;
    private readonly YamlPipelineEvaluator _sut;

    public YamlPipelineEvaluatorTests()
    {
        _azdoRestClientMock = new Mock<IAzdoRestClient>();
        _yamlHelperMock = new Mock<IYamlHelper>();

        _sut = new YamlPipelineEvaluator(_azdoRestClientMock.Object, _yamlHelperMock.Object);
    }

    [Theory, InlineData("a.b.c@1", "c", true), InlineData("@1", "", true), InlineData("", "", true),
     InlineData("a.b.c", "c", true), InlineData("c", "c", true), InlineData("a.b.c@1", "a", false)]
    public void ContainsTaskName_ShouldReturnTaskNameWithoutPrefixAndVersion(string taskName,
        string expectedTaskName, bool expectedResult) =>
        Assert.Equal(expectedResult, YamlPipelineEvaluator.ContainsTaskName(taskName, expectedTaskName));

    [Theory,
     InlineData("sonarQubeRunAnalysis", "true", true),
     InlineData("sonarQubeRunAnalysis", "True", true),
     InlineData("javaHomeOption", "JDKVersion", true),
     InlineData("javaHomeOption", "jDKVersion", false)]
    public void VerifyRuleInputs_ShouldNotBeCaseSensitiveForBooleanInput(string key, string value, bool expected)
    {
        // arrange
        var pipelineInputs = JToken.Parse(@"
{
    ""pomFile"": ""pom.xml"",
    ""goals"": ""deploy"",
    ""options"": "" - ntp -s $(mvn_settings.secureFilePath)"",
    ""publishJUnitResults"": ""true"",
    ""testResultsFiles"": "" * */surefire-reports/TEST-*.xml"",
    ""javaHomeOption"": ""JDKVersion"",
    ""jdkVersionOption"": ""1.11"",
    ""mavenVersionOption"": ""Default"",
    ""mavenAuthenticateFeed"": ""false"",
    ""effectivePomSkip"": ""false"",
    ""sonarQubeRunAnalysis"": ""True""
}");

        var pipelineInputDictionary = pipelineInputs.ToInputsDictionary();
        var inputs = new Dictionary<string, string> { [key] = value };
        var stageId = _fixture.Create<string>();
        var pipelineHasTaskRule = new PipelineHasTaskRule(stageId)
        {
            Inputs = inputs,
            IgnoreInputValues = false
        };

        // act
        var actual = YamlPipelineEvaluator.VerifyRuleInputs(pipelineHasTaskRule, pipelineInputDictionary);

        // assert
        actual.Should().Be(expected);
    }

    [Fact]
    public async Task EvaluateAsync_WithNoOrganizationName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var projectId = _fixture.Create<string>();
        var buildDefinition = _fixture.Create<BuildDefinition>();
        var pipelineHasTaskRule = _fixture.Create<PipelineHasTaskRule>();

        // Act
        Func<Task<bool>> actual = () => _sut.EvaluateAsync(null, projectId, buildDefinition, pipelineHasTaskRule);

        // Assert
        await actual.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EvaluateAsync_WithNoProjectId_ShouldThrowArgumentNullException()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var buildDefinition = _fixture.Create<BuildDefinition>();
        var pipelineHasTaskRule = _fixture.Create<PipelineHasTaskRule>();

        // Act
        Func<Task<bool>> actual = () => _sut.EvaluateAsync(organization, null, buildDefinition, pipelineHasTaskRule);

        // Assert
        await actual.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EvaluateAsync_WithNoBuildDefinition_ShouldThrowArgumentNullException()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipelineHasTaskRule = _fixture.Create<PipelineHasTaskRule>();

        // Act
        Func<Task<bool>> actual = () => _sut.EvaluateAsync(organization, projectId, null, pipelineHasTaskRule);

        // Assert
        await actual.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EvaluateAsync_WithNoYamlFilenameInTheBuildDefinition_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();

        var buildProcess = _fixture.Build<BuildProcess>()
            .Without(x => x.YamlFilename).Create();
        var buildDefinition = _fixture.Build<BuildDefinition>()
            .With(x => x.Process, buildProcess)
            .Create();
        var pipelineHasTaskRule = _fixture.Create<PipelineHasTaskRule>();

        // Act
        Func<Task<bool>> actual = () => _sut.EvaluateAsync(organization, projectId, buildDefinition, pipelineHasTaskRule);

        // Assert
        await actual.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task EvaluateAsync_WithNoPipelineHasTaskRule_ShouldThrowArgumentNullException()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var buildDefinition = _fixture.Create<BuildDefinition>();

        // Act
        Func<Task<bool>> actual = () => _sut.EvaluateAsync(organization, projectId, buildDefinition, null);

        // Assert
        await actual.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void VerifyRuleInputs()
    {
        // Arrange
        var pipelineHasTaskRule = _fixture.Build<PipelineHasTaskRule>()
            .Without(x => x.Inputs).Create();

        // Act
        var actual = YamlPipelineEvaluator.VerifyRuleInputs(pipelineHasTaskRule, new Dictionary<string, string>());

        // Assert
        actual.Should().BeFalse();
    }
}