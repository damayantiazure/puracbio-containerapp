using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Rabobank.Compliancy.Functions.PipelineBreaker.Tests.Model;

public class PipelineBreakerReportTests
{
    private static readonly IFixture Fixture = new Fixture();

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, false, false)]
    public void PipelineBreakerReport_ResultPassed(bool isExcluded, bool throwWarning, bool isBlockingEnabled)
    {
        // Arrange & Act
        var report = new PipelineBreakerReport
        {
            Result = PipelineBreakerExtensions.GetResult(isExcluded, new List<RuleCompliancyReport>(), throwWarning, isBlockingEnabled)
        };

        // Assert
        Assert.Equal(PipelineBreakerResult.Passed, report.Result);
    }

    [Fact]
    public void PipelineBreakerReport_IsNotExcludedAndAllRulesCompliant_ResultPassed()
    {
        // Arrange
        var ruleCompliancyReports = Fixture.Build<RuleCompliancyReport>()
        .With(r => r.IsCompliant, true)
        .CreateMany(1)
        .ToList();

        // Act
        var report = new PipelineBreakerReport
        {
            Result = PipelineBreakerExtensions.GetResult(false, ruleCompliancyReports, It.IsAny<bool>(), It.IsAny<bool>())
        };

        // Assert
        Assert.Equal(PipelineBreakerResult.Passed, report.Result);
    }

    [Fact]
    public void PipelineBreakerReport_IsNotExcludedAndNoRulesCompliantAndBlockingEnabled_ResultBlocked()
    {
        // Arrange
        var ruleCompliancyReports = Fixture.Build<RuleCompliancyReport>()
        .With(r => r.IsCompliant, false)
        .With(r => r.HasDeviation, false)
        .CreateMany(1)
        .ToList();

        // Act
        var report = new PipelineBreakerReport
        {
            Result = PipelineBreakerExtensions.GetResult(false, ruleCompliancyReports, It.IsAny<bool>(), true)
        };

        // Assert
        Assert.Equal(PipelineBreakerResult.Blocked, report.Result);
    }

    [Fact]
    public void PipelineBreakerReport_IsNotExcludedAndNoRulesCompliantAndTrowsWarningsAndBlockingDisabled_ResultBlocked()
    {
        // Arrange
        var ruleCompliancyReports = Fixture.Build<RuleCompliancyReport>()
        .With(r => r.IsCompliant, false)
        .With(r => r.HasDeviation, false)
        .CreateMany(1)
        .ToList();

        // Act
        var report = new PipelineBreakerReport
        {
            Result = PipelineBreakerExtensions.GetResult(false, ruleCompliancyReports, true, false)
        };

        // Assert
        Assert.Equal(PipelineBreakerResult.Warned, report.Result);
    }
}