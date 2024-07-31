using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Moq;
using Task = System.Threading.Tasks.Task;
using Xunit;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

namespace Rabobank.Compliancy.Infra.StorageClient.Tests;

public class PipelineRegistrationResolverTests
{
    [Fact]
    public async Task ResolveProductionStagesAsync_ProdAndNonProdPipelineRegistered_OneProdStageReturned()
    {
        var fixture = new Fixture();

        var organization = fixture.Create<string>();
        var projectId = fixture.Create<string>();
        var pipelineId = fixture.Create<string>();
        const string stageId = "55";
        var profile = fixture.Create<string>();
        var ciContentItem = new ConfigurationItem

        {
            CiID = "CI133333"
        };

        var items = new List<PipelineRegistration>
        {
            new PipelineRegistration(null, organization, projectId, pipelineId, null, stageId, profile),
            new PipelineRegistration(ciContentItem, organization, projectId, pipelineId, null, stageId, profile)
        };

        var repo = new Mock<IPipelineRegistrationRepository>();
        repo
            .Setup(x => x.GetAsync(organization, projectId))
            .ReturnsAsync(items);

        var resolver = new PipelineRegistrationResolver(repo.Object);
        var results = await resolver.ResolveProductionStagesAsync(organization, projectId, pipelineId);

        Assert.Equal(stageId, results.First());
        Assert.Single(results);
    }
}