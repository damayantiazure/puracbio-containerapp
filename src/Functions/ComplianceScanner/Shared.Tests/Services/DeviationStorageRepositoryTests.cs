#nullable enable

using AutoFixture;
using Microsoft.Azure.Cosmos.Table;
using Moq;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Tests.Services;

public class DeviationStorageRepositoryTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<CloudTableClient> _cloudTableClient;
    private readonly Mock<CloudTable> _cloudTable;

    public DeviationStorageRepositoryTests()
    {
        _cloudTableClient = new Mock<CloudTableClient>(_fixture.Create<Uri>(), 
            new StorageCredentials(_fixture.Create<string>(), _fixture.Create<string>()), null);
        _cloudTable = new Mock<CloudTable>(new Uri("http://localhost.com/FakeTable"), null);
    }

    [Fact]
    public async Task ShouldAddDeviationToTableStorage()
    {
        // Arrange
        var deviation = _fixture.Create<Deviation>();

        _cloudTableClient
            .Setup(x => x.GetTableReference(It.IsAny<string>()))
            .Returns(_cloudTable.Object)
            .Verifiable();

        var sut = new DeviationStorageRepository(() => _cloudTableClient.Object);

        // Act
        await sut.UpdateAsync(deviation);

        // Assert
        _cloudTableClient.Verify();
        _cloudTable
            .Verify(x => x.ExecuteAsync(It.Is<TableOperation>(t => 
                t.Entity.PartitionKey == deviation.PartitionKey && 
                t.Entity.RowKey == deviation.RowKey)), Times.Once);
    }

    [Fact]
    public async Task ShouldDeleteDeviationFromTableStorage()
    {
        // Arrange
        var deviation = new DynamicTableEntity(_fixture.Create<string>(), _fixture.Create<string>())
        {
            ETag = "*"
        };

        _cloudTableClient
            .Setup(x => x.GetTableReference(It.IsAny<string>()))
            .Returns(_cloudTable.Object)
            .Verifiable();

        var sut = new DeviationStorageRepository(() => _cloudTableClient.Object);

        // Act
        await sut.DeleteAsync(deviation);

        // Assert
        _cloudTableClient.Verify();
        _cloudTable
            .Verify(x => x.ExecuteAsync(It.Is<TableOperation>(t =>
                t.Entity.PartitionKey == deviation.PartitionKey &&
                t.Entity.RowKey == deviation.RowKey)), Times.Once);
    }
}