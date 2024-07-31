#nullable enable

using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Activities;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Tests.Activities;

public class UploadExceptionReportToLogAnalyticsActivityTests
{
    [Fact]
    public async Task DoesUploadExceptionReportToLogAnalytics()
    {
        // Arrange
        var fixture = new Fixture();
        var loggingServiceMock = new Mock<ILoggingService>();
        var exceptionReport = fixture.Create<ExceptionReport>();
        var activity = new UploadExceptionReportToLogAnalyticsActivity(loggingServiceMock.Object);

        // Act
        await activity.RunAsync(exceptionReport);

        // Assert
        loggingServiceMock.Verify(x => x.LogExceptionAsync(LogDestinations.ErrorHandlingLog,
            It.Is<ExceptionReport>(report => report.ExceptionMessage == exceptionReport.ExceptionMessage)), Times.Exactly(1));
    }
}