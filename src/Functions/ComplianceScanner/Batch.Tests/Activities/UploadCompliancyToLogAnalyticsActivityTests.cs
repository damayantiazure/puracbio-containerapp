#nullable enable

using System.Collections;
using System.Threading.Tasks;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Activities;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Tests.Activities;

public class UploadCompliancyToLogAnalyticsActivityTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task DoesUploadCompliancyToLogAnalytics()
    {
        // Arrange
        var loggingServiceMock = new Mock<ILoggingService>();
        var compliancyReport = _fixture.Create<CompliancyReport>();
        var projectId = _fixture.Create<string>();
        var sut = new UploadCompliancyToLogAnalyticsActivity(loggingServiceMock.Object);

        // Act
        await sut.RunAsync((_fixture.Create<string>(), projectId, compliancyReport));

        // Assert
        loggingServiceMock.Verify(loggingService => loggingService.LogInformationItemsAsync(
            LogDestinations.CompliancyRules, It.IsAny<IEnumerable>()), Times.Once);

        loggingServiceMock.Verify(loggingService => loggingService.LogInformationItemsAsync(
            LogDestinations.CompliancyPrinciples, It.IsAny<IEnumerable>()), Times.Once);

        loggingServiceMock.Verify(loggingService => loggingService.LogInformationItemsAsync(
            LogDestinations.CompliancyCis, It.IsAny<IEnumerable>()), Times.Once);

        loggingServiceMock.Verify(loggingService => loggingService.LogInformationItemsAsync(
            LogDestinations.CompliancyPipelines, It.IsAny<IEnumerable>()), Times.Once);

        loggingServiceMock.Verify(loggingService => loggingService.LogInformationItemsAsync(
            LogDestinations.CompliancyItems, It.IsAny<IEnumerable>()), Times.Once);
    }

    [Fact]
    public async Task CanHandleIncompletePrincipleReport()
    {
        // Arrange
        var loggingServiceMock = new Mock<ILoggingService>();
        var compliancyReport = _fixture.Create<CompliancyReport>();
        compliancyReport.RegisteredConfigurationItems![0].PrincipleReports = null;
        compliancyReport.RegisteredPipelines = null;
        compliancyReport.UnregisteredPipelines = null;

        var projectId = _fixture.Create<string>();
        var sut = new UploadCompliancyToLogAnalyticsActivity(loggingServiceMock.Object);

        // Act
        await sut.RunAsync((_fixture.Create<string>(), projectId, compliancyReport));

        // Assert
        loggingServiceMock.Verify(loggingService => loggingService.LogInformationItemsAsync(
            LogDestinations.CompliancyRules, It.IsAny<IEnumerable>()), Times.Once);

        loggingServiceMock.Verify(loggingService => loggingService.LogInformationItemsAsync(
            LogDestinations.CompliancyPrinciples, It.IsAny<IEnumerable>()), Times.Once);

        loggingServiceMock.Verify(loggingService => loggingService.LogInformationItemsAsync(
            LogDestinations.CompliancyCis, It.IsAny<IEnumerable>()), Times.Once);

        loggingServiceMock.Verify(loggingService => loggingService.LogInformationItemsAsync(
            LogDestinations.CompliancyPipelines, It.IsAny<IEnumerable>()), Times.Never);

        loggingServiceMock.Verify(loggingService => loggingService.LogInformationItemsAsync(
            LogDestinations.CompliancyItems, It.IsAny<IEnumerable>()), Times.Once);
    }
}