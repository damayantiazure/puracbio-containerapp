using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Functions.PipelineBreaker.Model;
using Shouldly;
using System.Collections.Generic;
using static Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model.Constants;

namespace Rabobank.Compliancy.Functions.PipelineBreaker.Tests.Model;

public class CompliancyResultMessagesTests
{
    [Fact]
    public void GetResultMessage_PassedResult_ReturnsAllowedToContinueMessage()
    {
        // Arrange
        var report = new PipelineBreakerReport
        {
            Result = PipelineBreakerResult.Passed
        };
            
        // Act
        var message = ComplianceResultMessages.GetResultMessage(report);

        // Assert
        message.ShouldBe(DecoratorResultMessages.Passed);
    }

    [Fact]
    public void GetResultMessage_WarnedResult_ReturnsWarningMessage()
    {
        // Arrange
        var report = new PipelineBreakerReport
        {
            Result = PipelineBreakerResult.Warned,
            RuleCompliancyReports = CreateRuleReports(false, false)
        };

        // Act
        var message = ComplianceResultMessages.GetResultMessage(report);

        // Assert
        message.ShouldBe(@$"{DecoratorResultMessages.WarningNotCompliant}
Rule: 'unittest' for 'itemname' is incompliant. 
For more information on how to become compliant, visit: {ConfluenceLinks.CompliancyDocumentation} 
");
    }

    [Fact]
    public void GetResultMessage_BlockedResult_ReturnsErrorMessage()
    {
        // Arrange
        var report = new PipelineBreakerReport
        {
            Result = PipelineBreakerResult.Blocked,
            RuleCompliancyReports = CreateRuleReports(false, false)
        };

        // Act
        var message = ComplianceResultMessages.GetResultMessage(report);

        // Assert
        message.ShouldBe(@$"{DecoratorResultMessages.NotCompliant}
Rule: 'unittest' for 'itemname' is incompliant. 
For more information on how to become compliant, visit: {ConfluenceLinks.CompliancyDocumentation} 
");
    }

    [Fact]
    public void GetResultMessage_IsExcludedResult_ReturnsValidExclusionMessage()
    {
        // Arrange
        var report = new PipelineBreakerReport
        {
            Result = PipelineBreakerResult.Blocked,
            RuleCompliancyReports = CreateRuleReports(false, false),
            IsExcluded = true
        };

        // Act
        var message = ComplianceResultMessages.GetResultMessage(report);

        // Assert
        message.ShouldBe(DecoratorResultMessages.ExclusionList);
    }

    private static IEnumerable<RuleCompliancyReport> CreateRuleReports(bool hasDeviation, bool isCompliant)
    {
        return new List<RuleCompliancyReport>
        {
            new RuleCompliancyReport
            {
                HasDeviation = hasDeviation,
                IsCompliant = isCompliant,
                RuleDescription = "unittest",
                ItemName = "itemname"
            }
        };
    }
}