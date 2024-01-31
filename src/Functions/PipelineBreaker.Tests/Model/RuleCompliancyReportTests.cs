using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Shouldly;

namespace Rabobank.Compliancy.Functions.PipelineBreaker.Tests.Model;

public class RuleCompliancyReportTests
{
    [Fact]
    public void ToString_NoDeviationCompliant_ReturnCompliant()
    {
        // Arrange
        const string ruleDescription = "unittest";
        const string itemName = "itemname";
        var report = new RuleCompliancyReport { HasDeviation = false, IsCompliant = true, RuleDescription = ruleDescription, ItemName = itemName };

        // Act
        var result = report.ToString();

        // Assert
        result.ShouldBe($"Rule: '{ruleDescription}' for '{itemName}' is compliant. ");
    }

    [Fact]
    public void ToString_HasDeviationIncompliant_ReturnCompliantWithDeviation()
    {
        // Arrange
        const string ruleDescription = "unittest";
        const string itemName = "itemname";
        var report = new RuleCompliancyReport { HasDeviation = true, IsCompliant = false, RuleDescription = ruleDescription, ItemName = itemName };

        // Act
        var result = report.ToString();

        // Assert
        result.ShouldBe($"Rule: '{ruleDescription}' for '{itemName}' is compliant with deviation. ");
    }

    [Fact]
    public void ToString_NoDeviationIncompliant_ReturnIncompliant()
    {
        // Arrange
        const string ruleDescription = "unittest";
        const string itemName = "itemname";
        var report = new RuleCompliancyReport { HasDeviation = false, IsCompliant = false, RuleDescription = ruleDescription, ItemName = itemName };

        // Act
        var result = report.ToString();

        // Assert
        result.ShouldBe($"Rule: '{ruleDescription}' for '{itemName}' is incompliant. ");
    }

    [Fact]
    public void ToString_HasDeviationCompliant_ReturnCompliant()
    {
        // Arrange
        const string ruleDescription = "unittest";
        const string itemName = "itemname";
        var report = new RuleCompliancyReport { HasDeviation = true, IsCompliant = true, RuleDescription = ruleDescription, ItemName = itemName };

        // Act
        var result = report.ToString();

        // Assert
        result.ShouldBe($"Rule: '{ruleDescription}' for '{itemName}' is compliant. ");
    }
}