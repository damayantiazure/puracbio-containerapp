#nullable enable

using AutoFixture;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Moq;
using Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Rabobank.Compliancy.Infra.StorageClient.Tests;

public class PipelineRegistrationStorageRepositoryTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public async Task CanAddBatchAsync()
    {
        // Arrange
        const string pipelineId = "1";
        const string pipelineType = "classic";
        const string organization = "raboweb-test";
        const string projectId = "2";

        var cloudTableMock = new Mock<CloudTable>(new Uri("http://unittests.localhost.com/FakeTable"), null);

        cloudTableMock.Setup(table => table.ExecuteBatchAsync(It.IsAny<TableBatchOperation>()));

        var cloudTableClientMock = new Mock<CloudTableClient>(new Uri("http://localhost")
            , new StorageCredentials(_fixture.Create<string>(), _fixture.Create<string>()), null);

        cloudTableClientMock.Setup(client => client.GetTableReference(It.IsAny<string>()))
            .Returns(cloudTableMock.Object);

        _fixture.Customize<PipelineRegistration>(s => s
            .With(e => e.PartitionKey, "NON-PROD")
            .With(e => e.PipelineId, pipelineId)
            .With(e => e.Organization, organization)
            .With(e => e.PipelineType, pipelineType)
            .With(e => e.ProjectId, projectId));
        var nonProdEntities = _fixture.CreateMany<PipelineRegistration>(1).ToList();

        _fixture.Customize<PipelineRegistration>(s => s
            .With(e => e.PartitionKey, "PROD")
            .With(e => e.PipelineId, "10"));

        var prodEntity = _fixture.Create<PipelineRegistration>();

        var prodEntities = new List<PipelineRegistration> { prodEntity };
        var ctor = typeof(TableQuerySegment<PipelineRegistration>)
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
            .First(c => c.GetParameters().Length == 1);

        var tableQueryResponseNonProd =
            ctor.Invoke(new object[] { nonProdEntities }) as TableQuerySegment<PipelineRegistration>;
        var tableQueryResponseProd =
            ctor.Invoke(new object[] { prodEntities }) as TableQuerySegment<PipelineRegistration>;

        cloudTableMock.Setup(m => m.ExecuteQuerySegmentedAsync(
            It.Is<TableQuery<PipelineRegistration>>(q => q.FilterString == "PartitionKey eq 'NON-PROD'"),
            It.IsAny<TableContinuationToken>())).ReturnsAsync(tableQueryResponseNonProd);

        cloudTableMock.Setup(m => m.ExecuteQuerySegmentedAsync(
            It.Is<TableQuery<PipelineRegistration>>(q => q.FilterString == "PartitionKey eq 'PROD'"),
            It.IsAny<TableContinuationToken>())).ReturnsAsync(tableQueryResponseProd);

        _fixture.Customize<PipelineRegistration>(s => s
            .With(e => e.PartitionKey, _fixture.Create<string>()));
        var entities = _fixture.CreateMany<PipelineRegistration>(143);
        var pipelineReg = _fixture.Create<PipelineRegistration>();
        pipelineReg.PartitionKey = "PROD";
        pipelineReg.PipelineId = pipelineId;
        pipelineReg.Organization = organization;
        pipelineReg.PipelineType = pipelineType;
        pipelineReg.ProjectId = projectId;
        entities = entities.Append(pipelineReg);

        var sut = new PipelineRegistrationStorageRepository(
            Mock.Of<ILogger<PipelineRegistrationStorageRepository>>(),
            () => cloudTableClientMock.Object);

        // Act
        await sut.AddBatchAsync(entities);

        // Assert
        // Check if 3 batches are processed. Batched are divided based on partition key and number (100)
        cloudTableMock.Verify(x => x.ExecuteBatchAsync(
            It.Is<TableBatchOperation>(b => b.All(
                o => o.OperationType == TableOperationType.InsertOrReplace))), Times.Exactly(3));

        // There must be 2 deletions. One for the registration which is double (PROD and NON-PROD)
        // and one for a PROD registration only present in tables storage but not in SM9
        cloudTableMock.Verify(x => x.ExecuteBatchAsync(
            It.Is<TableBatchOperation>(b => b.All(
                o => o.OperationType == TableOperationType.Delete))), Times.Exactly(2));
    }

    [Theory]
    [InlineData("PROD", 3)]
    [InlineData("NON-PROD", 0)]
    public async Task CanClearAsync(string partitionKey, int numTableBatchOperation)
    {
        // Arrange
        var cloudTableMock = new Mock<CloudTable>(new Uri("http://unittests.localhost.com/FakeTable"), null);

        _fixture.Customize<PipelineRegistration>(s => s
            .With(e => e.PartitionKey, partitionKey));
        var entities = _fixture.CreateMany<PipelineRegistration>(243).ToList();

        var ctor = typeof(TableQuerySegment<PipelineRegistration>)
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
            .First(c => c.GetParameters().Length == 1);

        var tableQueryResponse = ctor.Invoke(new object[] { entities }) as TableQuerySegment<PipelineRegistration>;

        cloudTableMock.Setup(table => table.ExecuteBatchAsync(It.IsAny<TableBatchOperation>()));
        cloudTableMock.Setup(table => table.ExecuteQuerySegmentedAsync(
            It.IsAny<TableQuery<PipelineRegistration>>(),
            It.IsAny<TableContinuationToken>())).ReturnsAsync(tableQueryResponse);

        var cloudTableClientMock = new Mock<CloudTableClient>(new Uri("http://localhost")
            , new StorageCredentials("blah", "blah"), null);

        cloudTableClientMock.Setup(client => client.GetTableReference(It.IsAny<string>()))
            .Returns(cloudTableMock.Object);

        var sut = new PipelineRegistrationStorageRepository(
            Mock.Of<ILogger<PipelineRegistrationStorageRepository>>(),
            () => cloudTableClientMock.Object);

        // Act
        await sut.ClearAsync();

        // Assert
        cloudTableMock.Verify(
            x => x.ExecuteBatchAsync(It.Is<TableBatchOperation>(
                b => b.All(o => o.OperationType == TableOperationType.Delete &&
                                o.Entity.PartitionKey == "PROD"))), Times.Exactly(numTableBatchOperation));
    }

    [Fact]
    public async Task CanInsertOrMergeEntityAsync()
    {
        // Arrange
        var cloudTableMock = new Mock<CloudTable>(new Uri("http://unittests.localhost.com/FakeTable"), null);
        var cloudTableClientMock = new Mock<CloudTableClient>(new Uri("http://localhost")
            , new StorageCredentials("blah", "blah"), null);
        cloudTableClientMock.Setup(client => client.GetTableReference(It.IsAny<string>()))
            .Returns(cloudTableMock.Object);
        var sut = new PipelineRegistrationStorageRepository(
            Mock.Of<ILogger<PipelineRegistrationStorageRepository>>(), () => cloudTableClientMock.Object);
        var entity = new PipelineRegistration(_fixture.Create<ConfigurationItem>(), _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());

        // Act
        await sut.InsertOrMergeEntityAsync(entity).ConfigureAwait(false);

        // Assert 
        cloudTableMock.Verify(
            x => x.ExecuteAsync(
                It.Is<TableOperation>(
                    to => to.Entity.PartitionKey == entity.PartitionKey && to.Entity.RowKey == entity.RowKey)),
            Times.Exactly(1));
    }
}