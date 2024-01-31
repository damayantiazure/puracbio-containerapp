using AutoMapper;
using Azure;
using Azure.Core;
using Azure.Monitor.Ingestion;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Infrastructure.Config;
using Rabobank.Compliancy.Infrastructure.Dto.Logging;
using Rabobank.Compliancy.Infrastructure.InternalContracts;
using Rabobank.Compliancy.Infrastructure.InternalServices;
using Rabobank.Compliancy.Infrastructure.Mapping;

namespace Rabobank.Compliancy.Infrastructure.Tests;

public class LogIngestionServiceTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IIngestionClientFactory> _ingestionClientFactoryMock = new();
    private readonly Mock<LogsIngestionClient> _logIngestionClientMock = new();
    private readonly Mock<LogIngestionClientConfig> _logWriterClientConfigMock = new();

    [Fact]
    public async Task SendLogEntryAsync_ShouldCall_LogsIngestionClient()
    {
        // Arrange
        var entity = _fixture.Create<AuditLoggingReport>();
        var sut = CreateSut();

        // Act
        await sut.WriteLogEntryAsync(entity, LogDestinations.AuditDeploymentLog);

        // Assert
        _logIngestionClientMock.Verify(
            m => m.UploadAsync(_logWriterClientConfigMock.Object.RuleId, _logWriterClientConfigMock.Object.StreamName,
                It.IsAny<RequestContent>(), It.IsAny<string>(), It.IsAny<RequestContext>()), Times.Once);
    }

    [Fact]
    public async Task SendLogEntriesAsync_ShouldCall_LogsIngestionClient()
    {
        // Arrange
        var entities = _fixture.CreateMany<AuditLoggingReport>().ToList();
        var sut = CreateSut();

        // Act
        await sut.WriteLogEntriesAsync(entities, LogDestinations.AuditDeploymentLog);

        // Assert
        _logIngestionClientMock.Verify(
            m => m.UploadAsync(_logWriterClientConfigMock.Object.RuleId, _logWriterClientConfigMock.Object.StreamName,
                It.IsAny<RequestContent>(), It.IsAny<string>(), It.IsAny<RequestContext>()), Times.Once);
    }

    private LogIngestionService CreateSut()
    {
        var mapper = new Mapper(new MapperConfiguration(c => c.AddProfile(new LoggingMappingProfile())));

        _ingestionClientFactoryMock.Setup(m => m.Create(nameof(AuditDeploymentLogDto)))
            .Returns(
                new IngestionClientFactoryResult(_logIngestionClientMock.Object, _logWriterClientConfigMock.Object));
        
        return new LogIngestionService(_ingestionClientFactoryMock.Object, mapper);
    }
}