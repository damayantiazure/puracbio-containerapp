using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy.Authorizations;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;

namespace Rabobank.Compliancy.Infrastructure.Tests;

public class LoggingServiceTests
{
    private const string _organization = "Raboweb";
    private const string _projectId = "1";
    private const string _itemId = "2";
    private const string _stageId = "5";
    private const string _ruleName = "RandomRule";
    private const string _functionName = "functionName";
    private const string _requestUrl = "requestUrl";
    private const string _ciIdentifier = "CI123456";
    private const string _itemType = "RandomItem";

    private readonly ExceptionBaseMetaInformation _exceptionBaseMetaInformation =
        new(_functionName, _organization, _projectId, _requestUrl);

    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<ILogIngestionService> _logIngestionServiceMock = new();

    [Fact]
    public async Task LogInformationAsync_DestinationProvided_LogsObjectToDestination()
    {
        // Arrange        
        var report = _fixture.Create<ExceptionBaseMetaInformation>();
        var sut = new LoggingService(_logIngestionServiceMock.Object);

        // Act
        await sut.LogInformationAsync(LogDestinations.AuditDeploymentLog, report);

        // Assert
        _logIngestionServiceMock.Verify(l => l.WriteLogEntryAsync(
            It.Is<object>(v => v == report), LogDestinations.AuditDeploymentLog));
    }

    [Fact]
    public async Task ShouldLogExceptionReport_MinimalReport_LogsReportCorrectly()
    {
        // Arrange
        var exception = _fixture.Create<ArgumentNullException>();
        var sut = new LoggingService(_logIngestionServiceMock.Object);

        // Act
        await sut.LogExceptionAsync(LogDestinations.AuditDeploymentLog, _exceptionBaseMetaInformation, exception);

        // Assert
        _logIngestionServiceMock
            .Verify(m => m.WriteLogEntryAsync(It.Is<ExceptionReport>(r =>
                r.FunctionName == _functionName &&
                r.RequestUrl == _requestUrl &&
                r.Organization == _organization &&
                r.ProjectId == _projectId), LogDestinations.AuditDeploymentLog), Times.Once);
    }

    [Fact]
    public async Task ShouldLogExceptionReport_ReportWithCi_LogsReportCorrectly()
    {
        // Arrange
        var exception = _fixture.Create<ArgumentNullException>();
        var sut = new LoggingService(_logIngestionServiceMock.Object);

        // Act
        await sut.LogExceptionAsync(LogDestinations.AuditDeploymentLog, _exceptionBaseMetaInformation, exception,
            _ciIdentifier);

        // Assert
        _logIngestionServiceMock
            .Verify(m => m.WriteLogEntryAsync(It.Is<ExceptionReport>(r =>
                    r.FunctionName == _functionName &&
                    r.RequestUrl == _requestUrl &&
                    r.Organization == _organization &&
                    r.ProjectId == _projectId &&
                    r.CiIdentifier == _ciIdentifier),
                LogDestinations.AuditDeploymentLog), Times.Once);
    }

    [Fact]
    public async Task ShouldLogExceptionReport_ReportWithUser_LogsReportCorrectly()
    {
        // Arrange
        var exception = _fixture.Create<ArgumentNullException>();
        var user = _fixture.Build<User>()
            .FromFactory<string, Guid>((displayName, uniqueId) => new User(displayName, uniqueId.ToString()))
            .Create();

        var sut = new LoggingService(_logIngestionServiceMock.Object);

        // Act
        await sut.LogExceptionAsync(LogDestinations.AuditDeploymentLog, _exceptionBaseMetaInformation,
            exception, user, _itemId, _stageId, _ciIdentifier);

        // Assert
        _logIngestionServiceMock
            .Verify(m => m.WriteLogEntryAsync(It.Is<ExceptionReport>(exceptionReport =>
                    exceptionReport.FunctionName == _functionName &&
                    exceptionReport.RequestUrl == _requestUrl &&
                    exceptionReport.Organization == _organization &&
                    exceptionReport.ProjectId == _projectId &&
                    exceptionReport.ItemId == _itemId &&
                    exceptionReport.StageId == _stageId &&
                    exceptionReport.CiIdentifier == _ciIdentifier &&
                    exceptionReport.UserId == Guid.Parse(user.UniqueId) &&
                    exceptionReport.UserMail == user.MailAddress),
                LogDestinations.AuditDeploymentLog), Times.Once);
    }

    [Fact]
    public async Task ShouldLogExceptionReport_ReportWithRule_LogsReportCorrectly()
    {
        // Arrange
        var exception = _fixture.Create<ArgumentNullException>();
        var sut = new LoggingService(_logIngestionServiceMock.Object);

        // Act
        await sut.LogExceptionAsync(LogDestinations.AuditDeploymentLog, _exceptionBaseMetaInformation,
            _itemId, _ruleName, exception);

        // Assert
        _logIngestionServiceMock
            .Verify(m => m.WriteLogEntryAsync(It.Is<ExceptionReport>(r =>
                    r.FunctionName == _functionName &&
                    r.RequestUrl == _requestUrl &&
                    r.Organization == _organization &&
                    r.ProjectId == _projectId &&
                    r.ItemId == _itemId &&
                    r.RuleName == _ruleName),
                LogDestinations.AuditDeploymentLog), Times.Once);
    }

    [Fact]
    public async Task ShouldLogExceptionReport_ReportWithItemType_LogsReportCorrectly()
    {
        // Arrange        
        var exception = _fixture.Create<ArgumentNullException>();
        var sut = new LoggingService(_logIngestionServiceMock.Object);

        // Act
        await sut.LogExceptionAsync(LogDestinations.AuditDeploymentLog, exception, _exceptionBaseMetaInformation,
            _itemId, _itemType);

        // Assert
        _logIngestionServiceMock
            .Verify(m => m.WriteLogEntryAsync(It.Is<ExceptionReport>(r =>
                    r.FunctionName == _functionName &&
                    r.RequestUrl == _requestUrl &&
                    r.Organization == _organization &&
                    r.ProjectId == _projectId &&
                    r.ItemId == _itemId &&
                    r.ItemType == _itemType),
                LogDestinations.AuditDeploymentLog), Times.Once);
    }
}