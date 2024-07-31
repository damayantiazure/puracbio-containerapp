#nullable enable

using System;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Rabobank.Compliancy.Core.InputValidation.Model;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Activities;
using Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Orchestrators;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Tests.Orchestrators;

public class ProjectScanOrchestratorTests
{
    private readonly Mock<IDurableOrchestrationContext> _context = new();
    private readonly IFixture _fixture = new Fixture { RepeatCount = 1 };

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 1)]
    public async Task ShouldCallAllActivities_AndUploadLogErrorReportForFailures(
        bool ciScanIsFailed, int expectedCalls)
    {
        //Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Create<Project>();
        var scanDate = _fixture.Create<DateTime>();
        var input = (organization, project, scanDate);
        var exception = _fixture.Create<ExceptionSummaryReport>();

        _fixture.Customize<CiReport>(customizationComposer => customizationComposer
            .With(ciReport => ciReport.IsScanFailed, ciScanIsFailed)
            .With(ciReport => ciReport.ScanException, exception));

        var report = _fixture.Create<CompliancyReport>();
        var inputUploadActivity = (organization, project.Id, report);

        _context
            .Setup(x => x.GetInput<(string, Project, DateTime)>())
            .Returns(input)
            .Verifiable();
        _context
            .Setup(x => x.CallActivityWithRetryAsync<CompliancyReport>(nameof(ScanProjectActivity),
                It.IsAny<RetryOptions>(), input))
            .ReturnsAsync(report)
            .Verifiable();
        _context
            .Setup(x => x.CallActivityWithRetryAsync(nameof(UploadCompliancyToLogAnalyticsActivity),
                It.IsAny<RetryOptions>(), inputUploadActivity))
            .Verifiable();

        //Act
        var function = new ProjectScanOrchestrator();
        await function.RunAsync(_context.Object);

        //Assert
        _context.Verify();

        var expectedMessage = $"Exception: {exception.ExceptionMessage}. Innerexception: " +
                              $"{exception.InnerExceptionMessage}";

        _context
            .Verify(durableOrchestrationContext => durableOrchestrationContext.CallActivityWithRetryAsync(
                nameof(UploadExceptionReportToLogAnalyticsActivity),
                It.IsAny<RetryOptions>(), It.Is<ExceptionReport>(exceptionReport =>
                    exceptionReport.ExceptionMessage!.Contains(expectedMessage))), Times.Exactly(expectedCalls));
    }
}