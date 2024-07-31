#nullable enable

using Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Activities;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using System;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Tests.Activities;

public class ScanProjectActivityTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IScanProjectService> _scanProjectService = new Mock<IScanProjectService>();

    [Fact]
    public async Task ShouldCallScanProjectService()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Create<Project>();
        var scanDate = _fixture.Create<DateTime>();
        var report = _fixture.Create<CompliancyReport>();

        _scanProjectService
            .Setup(x => x.ScanProjectAsync(organization, project, scanDate, It.IsAny<int>()))
            .ReturnsAsync(report)
            .Verifiable();

        // Act
        var activity = new ScanProjectActivity(_scanProjectService.Object);
        var result = await activity.RunAsync((organization, project, scanDate));

        // Assert
        result.ShouldBe(report);
        _scanProjectService.Verify();
    }
}