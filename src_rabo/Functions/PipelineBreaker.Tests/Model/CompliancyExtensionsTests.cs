using Rabobank.Compliancy.Domain.Compliancy.Reports;

namespace Rabobank.Compliancy.Functions.PipelineBreaker.Tests.Model;

public class CompliancyExtensionsTests
{
    [Fact]
    public void IsDeterminedCompliant_IncompliantNoDeviation_ReturnsFalse()
    {
        // Arrange
        var compliancyReport = new RuleCompliancyReport { IsCompliant = false, HasDeviation = false };

        // Act
        var result = compliancyReport.IsDeterminedCompliant();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsDeterminedCompliant_IncompliantWithDeviation_ReturnsTrue()
    {
        // Arrange
        var compliancyReport = new RuleCompliancyReport { IsCompliant = false, HasDeviation = true };

        // Act
        var result = compliancyReport.IsDeterminedCompliant();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsDeterminedCompliant_CompliantWithDeviation_ReturnsTrue()
    {
        // Arrange
        var compliancyReport = new RuleCompliancyReport { IsCompliant = true, HasDeviation = true };

        // Act
        var result = compliancyReport.IsDeterminedCompliant();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsDeterminedCompliant_CompliantWithoutDeviation_ReturnsTrue()
    {
        // Arrange
        var compliancyReport = new RuleCompliancyReport { IsCompliant = true, HasDeviation = false };

        // Act
        var result = compliancyReport.IsDeterminedCompliant();

        // Assert
        Assert.True(result);
    }
}