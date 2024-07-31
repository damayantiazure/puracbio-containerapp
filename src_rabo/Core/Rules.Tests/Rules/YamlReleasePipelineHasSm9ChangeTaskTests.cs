using AutoFixture;
using NSubstitute;
using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.PipelineResources.Model;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Rules.Tests.Rules;

public class YamlReleasePipelineHasSm9ChangeTaskTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly IAzdoRestClient _azdoRestClient;
    private readonly IYamlHelper _yamlHelper;
    private readonly YamlReleasePipelineHasSm9ChangeTask _sut;

    public YamlReleasePipelineHasSm9ChangeTaskTests()
    {
        _azdoRestClient = Substitute.For<IAzdoRestClient>();
        _yamlHelper = Substitute.For<IYamlHelper>();
        _sut = new YamlReleasePipelineHasSm9ChangeTask(_azdoRestClient, _yamlHelper);
    }

    [Theory]
    [InlineData("tas.tastest.SM9Create.SM9 - Create@1", true)]
    [InlineData("tas.tastest.SM9Create.SM9 - Approve@2", true)]
    [InlineData("", false)]
    public async Task GivenPipeline_WhenYamlWithSm9Task_ThenEvaluatesToTrue(string task,
        bool isCompliant)
    {
        // Arrange
        var buildPipeline = _fixture.Create<BuildDefinition>();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipelineTaskInputs = _fixture.Build<PipelineTaskInputs>()
            .With(x => x.Enabled, true)
            .With(x => x.FullTaskName, task).CreateMany(1);

        _yamlHelper.GetPipelineTasksAsync(organization, projectId, buildPipeline)
            .Returns(pipelineTaskInputs);

        // Act
        var actual = await _sut.EvaluateAsync(organization, projectId, buildPipeline);

        // Assert
        actual.ShouldBe(isCompliant);
    }
}