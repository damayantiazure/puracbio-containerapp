using Moq;
using Rabobank.Compliancy.Core.Rules.Exceptions;
using Rabobank.Compliancy.Core.Rules.Helpers;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.StorageClient;
using System.Linq;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Rules.Tests.Helpers;

public class YamlEnvironmentHelperTests
{
    [Fact]
    public async Task GetProdEnvironmentsAsync_RegisteredStageDifferentCasing_ReturnsEnvironment()
    {
        // Arrange
        const string environmentName = "prod";
        const string organization = "raboweb-test";
        const string pipelineId = "2";
        const string projectId = "1";

        var pipelineResolverMock = new Mock<IPipelineRegistrationResolver>();
        var azdoClientMock = new Mock<IAzdoRestClient>();
        var pipelineDefinition = new BuildDefinition
        {
            PipelineType = ItemTypes.BuildPipeline,
            YamlUsedInRun = $"stages:\r\n- stage: stage1\r\n  jobs:\r\n  -  environment: '{environmentName}'",
            Id = pipelineId
        };

        var yamlHelper = new YamlEnvironmentHelper(azdoClientMock.Object, pipelineResolverMock.Object);
        pipelineResolverMock
            .Setup(m => m.ResolveProductionStagesAsync(organization, projectId, pipelineId))
            .ReturnsAsync(new[] { "Stage1" });

        azdoClientMock
            .Setup(m => m.GetAsync(It.IsAny<IEnumerableRequest<EnvironmentYaml>>(), organization))
            .ReturnsAsync(new[] { new EnvironmentYaml { Id = 1, Name = environmentName } });

        // Act
        var result = await yamlHelper.GetProdEnvironmentsAsync(organization, projectId, pipelineDefinition);

        // Assert
        result.Count().ShouldBe(1);
        result.First().Name.ShouldBe(environmentName);
    }
}