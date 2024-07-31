#nullable enable

using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Azure.Cosmos.Table;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using Shouldly;
using Xunit;

namespace Rabobank.Compliancy.Infra.StorageClient.Tests;

public class PipelineRegistrationRepositoryIntegrationTests
{
    private const string _tableName = "DeploymentMethod";

    [Fact]
    [Trait("category", "integration")]
    public async Task ShouldReturnEmptyListIfTableDoesNotExist()
    {
        // Arrange 
        // Use Azurite to use local development server
        //   a) npm i -g azurite@2
        //   b) docker run --rm -p 10007:10007 arafato/azurite

        var storage = CloudStorageAccount.Parse(TestConfiguration.ConnectionString);
        var client = storage.CreateCloudTableClient();

        var fixture = new Fixture();
        var projectId = fixture.Create<string>();
        var organization = fixture.Create<string>();

        var sut = new PipelineRegistrationRepository(() => client);

        // Act
        var actual = await sut.GetAsync(organization, projectId);

        // Assert
        actual.Count.ShouldBe(0);
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task ShouldReturnEmptyListIfTableColumnDoesNotExist()
    {
        // Arrange 
        var storage = CloudStorageAccount.Parse(TestConfiguration.ConnectionString);
        var client = storage.CreateCloudTableClient();
        var table = client.GetTableReference(_tableName);
        await table.CreateIfNotExistsAsync();

        var fixture = new Fixture();
        var projectId = fixture.Create<string>();
        var organization = fixture.Create<string>();

        var sut = new PipelineRegistrationRepository(() => client);

        // Act
        var actual = await sut.GetAsync(organization, projectId);

        // Assert
        actual.Count.ShouldBe(0);
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task ShouldReturnProductionItemsForProjectWithinOrganization()
    {
        // Arrange 
        var storage = CloudStorageAccount.Parse(TestConfiguration.ConnectionString);
        var client = storage.CreateCloudTableClient();
        var table = client.GetTableReference(_tableName);

        var fixture = new Fixture();
        var projectId = fixture.Create<string>();
        var organization = fixture.Create<string>();
        var numberOfRows = fixture.Create<int>();

        await CreateDummyTable(table, organization, projectId, numberOfRows);

        var sut = new PipelineRegistrationRepository(() => client);

        // Act
        var actual = await sut.GetAsync(organization, projectId);

        // Assert
        actual.Count.ShouldBe(numberOfRows);
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task ShouldReturnProductionItemsForBigTable()
    {
        // Arrange 
        var storage = CloudStorageAccount.Parse(TestConfiguration.ConnectionString);
        var client = storage.CreateCloudTableClient();
        var table = client.GetTableReference(_tableName);

        var fixture = new Fixture();
        var projectId = fixture.Create<string>();
        var organization = fixture.Create<string>();
        const int numberOfRows = 1500;

        await CreateDummyTable(table, organization, projectId, numberOfRows);

        var sut = new PipelineRegistrationRepository(() => client);

        // Act
        var actual = await sut.GetAsync(organization, projectId);

        // Assert
        actual.Count.ShouldBe(numberOfRows);
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task ShouldNotReturnProductionItemsForOtherProject()
    {
        // Arrange 
        var storage = CloudStorageAccount.Parse(TestConfiguration.ConnectionString);
        var client = storage.CreateCloudTableClient();
        var table = client.GetTableReference(_tableName);

        var fixture = new Fixture();
        var projectId = fixture.Create<string>();
        var organization = fixture.Create<string>();
        var numberOfRows = fixture.Create<int>();

        await CreateDummyTable(table, organization, "otherProjectId", numberOfRows);

        var sut = new PipelineRegistrationRepository(() => client);

        // Act
        var actual = await sut.GetAsync(organization, projectId);

        // Assert
        actual.Count.ShouldBe(0);
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task ShouldNotReturnProductionItemsForOtherOrganization()
    {
        // Arrange 
        var storage = CloudStorageAccount.Parse(TestConfiguration.ConnectionString);
        var client = storage.CreateCloudTableClient();
        var table = client.GetTableReference(_tableName);

        var fixture = new Fixture();
        var projectId = fixture.Create<string>();
        var organization = fixture.Create<string>();
        var numberOfRows = fixture.Create<int>();

        await CreateDummyTable(table, "otherOrganization", projectId, numberOfRows);

        var sut = new PipelineRegistrationRepository(() => client);

        // Act
        var actual = await sut.GetAsync(organization, projectId);

        // Assert
        actual.Count.ShouldBe(0);
    }

    private static async Task CreateDummyTable(CloudTable table, string organization,
        string projectId, int count)
    {
        await table.CreateIfNotExistsAsync();

        var fixture = new Fixture();
        fixture.Customize<PipelineRegistration>(ctx => ctx
            .With(x => x.Organization, organization)
            .With(x => x.ProjectId, projectId));

        foreach (var ci in fixture.CreateMany<PipelineRegistration>(count))
        {
            await table.ExecuteAsync(TableOperation.InsertOrReplace(ci));
        }
    }
}