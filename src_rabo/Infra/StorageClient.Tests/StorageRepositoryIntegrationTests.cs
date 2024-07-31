#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Azure.Cosmos.Table;
using Shouldly;
using Xunit;

namespace Rabobank.Compliancy.Infra.StorageClient.Tests;

#region README

/*for these integration tests to pass locally you need to start Azurite and create a local.settings.json with values:
"AzureWebJobsStorage": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10006/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10007/devstoreaccount1;TableEndpoint=http://127.0.0.1:10008/devstoreaccount1",
"FUNCTIONS_WORKER_RUNTIME": "dotnet",
This integration test works only via pipeline, this should be fixed later
 */

#endregion

public class StorageRepositoryIntegrationTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task StorageRepository_InsertOrReplaceAsync_InsertEntity()
    {
        // Arrange
        var partitionKey = _fixture.Create<string>();
        var rowKey = _fixture.Create<string>();

        var entity = new DynamicTableEntity(partitionKey, rowKey);
        entity.Properties.Add("PipelineId", EntityProperty.GeneratePropertyForString("pipelineId"));
        entity.Properties.Add("ProjectId", EntityProperty.GeneratePropertyForString("projectId"));
        entity.Properties.Add("CiName", EntityProperty.GeneratePropertyForString("CiName"));
        entity.Properties.Add("IsSoxApplication", EntityProperty.GeneratePropertyForBool(false));
        entity.Properties.Add("Organization", EntityProperty.GeneratePropertyForString("integrationTests"));
        entity.ETag = "*";

        var sut = CreateSut();

        // Act
        var actual = await sut.InsertOrReplaceAsync(entity);

        // Assert
        actual.HttpStatusCode.ShouldBe(204);
    }

    [Fact]
    public async Task StorageRepository_InsertOrMergeAsync()
    {
        // Arrange
        var partitionKey = _fixture.Create<string>();
        var rowKey = _fixture.Create<string>();

        var entity = new DynamicTableEntity(partitionKey, rowKey);
        entity.Properties.Add("PipelineId", EntityProperty.GeneratePropertyForString("pipelineId"));
        entity.Properties.Add("ProjectId", EntityProperty.GeneratePropertyForString("projectId"));
        entity.Properties.Add("CiName", EntityProperty.GeneratePropertyForString("CiName"));
        entity.Properties.Add("IsSoxApplication", EntityProperty.GeneratePropertyForBool(false));
        entity.Properties.Add("Organization", EntityProperty.GeneratePropertyForString("integrationTests"));
        entity.ETag = "*";

        var sut = CreateSut();

        // Act
        var actual = await sut.InsertOrMergeAsync(entity);

        // Assert
        actual.HttpStatusCode.ShouldBe(204);
    }

    [Fact]
    public async Task StorageRepository_DeleteAsync_DeleteEntity()
    {
        //assert
        var partitionKey1 = _fixture.Create<string>();
        var rowKey1 = _fixture.Create<string>();

        var partitionKey2 = _fixture.Create<string>();
        var rowKey2 = _fixture.Create<string>();

        var keysList = new Dictionary<string, string>
        {
            { partitionKey1, rowKey1 },
            { partitionKey2, rowKey2 }
        };

        var referencePartitionKey = keysList.Select(m => m.Key).Single(p => p == partitionKey2);
        var referenceRowKey = keysList.Select(m => m.Value).Single(p => p == rowKey2);

        var sut = CreateSut();

        await sut.GetEntityAsync(referencePartitionKey, referenceRowKey);

        foreach (var item in keysList)
        {
            var entitiesToInsert = new DynamicTableEntity(item.Key, item.Value);
            entitiesToInsert.Properties.Add("PipelineId", EntityProperty.GeneratePropertyForString("pipelineId"));
            entitiesToInsert.Properties.Add("ProjectId", EntityProperty.GeneratePropertyForString("projectId"));
            entitiesToInsert.Properties.Add("CiName", EntityProperty.GeneratePropertyForString("CiName"));
            entitiesToInsert.Properties.Add("IsSoxApplication", EntityProperty.GeneratePropertyForBool(false));
            entitiesToInsert.Properties.Add("Organization",
                EntityProperty.GeneratePropertyForString("integrationTests"));

            await sut.InsertOrReplaceAsync(entitiesToInsert);
        }

        var request = await DeleteData(sut, referencePartitionKey, referenceRowKey);

        var list = await sut.GetEntityAsync(
            keysList.Select(m => m.Key).FirstOrDefault(p => p == partitionKey1),
            keysList.Select(m => m.Value).FirstOrDefault(p => p == rowKey1));

        list!.Result.ShouldNotBeNull();
        request.HttpStatusCode.ShouldBe(204);
    }

    [Fact]
    public async Task StorageRepository_GetEntityAsync_ListEntityShouldNotBeNull()
    {
        // Arrange
        var partitionKey = _fixture.Create<string>();
        var rowKey = _fixture.Create<string>();

        var entityToInsert = new DynamicTableEntity(partitionKey, rowKey);
        entityToInsert.Properties.Add("PipelineId", EntityProperty.GeneratePropertyForString("pipelineId"));
        entityToInsert.Properties.Add("ProjectId", EntityProperty.GeneratePropertyForString("projectId"));
        entityToInsert.Properties.Add("CiName", EntityProperty.GeneratePropertyForString("CiName"));
        entityToInsert.Properties.Add("IsSoxApplication", EntityProperty.GeneratePropertyForBool(false));
        entityToInsert.Properties.Add("Organization", EntityProperty.GeneratePropertyForString("integrationTests"));

        var sut = CreateSut();

        var insertEntity = await sut.InsertOrReplaceAsync(entityToInsert);

        await DeleteData(sut, partitionKey, rowKey).ConfigureAwait(false);

        if (insertEntity.HttpStatusCode == 200)
        {
            // Act
            var actual = await sut.GetEntityAsync(partitionKey, rowKey);

            // Assert
            actual!.HttpStatusCode.ShouldBe(204);
            actual.Result.ShouldNotBeNull();
        }
    }

    private static StorageRepository CreateSut()
    {
        var storageRepository = new StorageRepository(() =>
            CloudStorageAccount.Parse(TestConfiguration.ConnectionString).CreateCloudTableClient());
        storageRepository.CreateTable("IntegrationTestsLocal");
        return storageRepository;
    }

    private static async Task<TableResult> DeleteData(IStorageRepository sut, string? referencePartitionKey,
        string? referenceRowKey)
    {
        var entity = new DynamicTableEntity(referencePartitionKey, referenceRowKey)
        {
            ETag = "*"
        };

        return await sut.DeleteAsync(entity);
    }
}