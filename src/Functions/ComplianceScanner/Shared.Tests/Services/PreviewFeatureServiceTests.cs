using AutoFixture;
using Microsoft.Azure.Cosmos.Table;
using Moq;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using Rabobank.Compliancy.Infra.StorageClient;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Tests.Services;

public class PreviewFeatureServiceTests
{
    private readonly Mock<IStorageRepository> _storageRepositoryMock = new();
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public async Task PreviewFeatureIsEnabled_ReturnsTrue()
    {
        // Arrange
        var featureName = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();

        var tableResult = new TableResult
        {
            Result = _fixture.Create<PreviewFeature>()
        };

        _storageRepositoryMock
            .Setup(m => m.GetEntityAsync<PreviewFeature>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(tableResult)
            .Verifiable();

        var service = new PreviewFeatureService(_storageRepositoryMock.Object);

        // Act
        var result = await service.PreviewFeatureEnabledAsync(featureName, projectId);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task PreviewFeatureIsNotEnabled_ReturnsFalse()
    {
        // Arrange
        var featureName = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();

        _storageRepositoryMock
            .Setup(m => m.GetEntityAsync<PreviewFeature>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((TableResult)null)
            .Verifiable();

        var service = new PreviewFeatureService(_storageRepositoryMock.Object);

        // Act
        var result = await service.PreviewFeatureEnabledAsync(featureName, projectId);

        // Assert
        result.ShouldBeFalse();
    }
}