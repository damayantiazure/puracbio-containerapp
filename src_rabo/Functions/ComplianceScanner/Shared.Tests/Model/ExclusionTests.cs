using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;
using Shouldly;
using System;
using Xunit;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Tests.Model;

public class ExclusionTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData("", null)]
    [InlineData(null, "")]
    [InlineData(null, null)]
    [InlineData("unittest", "")]
    [InlineData("unittest", null)]
    [InlineData(null, "unittest")]
    [InlineData("", "unittest")]
    public void IsApproved_NotApprovedExclusion_ReturnsFalse(string requester, string approver)
    {
        // Arrange
        var runInfo = new PipelineRunInfo("", "", "", "", "");
        var exclusion = new Exclusion(runInfo)
        {
            Requester = requester,
            Approver = approver
        };

        // Act
        var result = exclusion.IsApproved;

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsApproved_ApprovedExclusion_ReturnsTrue()
    {
        // Arrange
        var runInfo = new PipelineRunInfo("", "", "", "", "");
        var exclusion = new Exclusion(runInfo)
        {
            Requester = "unittest",
            Approver = "unittest2"
        };

        // Act
        var result = exclusion.IsApproved;

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsApproved_RequesterSameAsApprover_ReturnsFalse()
    {
        // Arrange
        var runInfo = new PipelineRunInfo("", "", "", "", "");
        var exclusion = new Exclusion(runInfo)
        {
            Requester = "unittest",
            Approver = "unittest"
        };

        // Act
        var result = exclusion.IsApproved;

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsExpired_WithinValidTimeFrame_ReturnsFalse()
    {
        // Arrange
        var runInfo = new PipelineRunInfo("", "", "", "", "");
        var exclusion = new Exclusion(runInfo);
        exclusion.Timestamp = DateTime.Now.AddHours(-8);

        // Act
        var result = exclusion.IsExpired(24);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsExpired_NotInValidTimeFrame_ReturnsTrue()
    {
        // Arrange
        var runInfo = new PipelineRunInfo("", "", "", "", "");
        var exclusion = new Exclusion(runInfo)
        {
            Timestamp = DateTime.Now.AddHours(-25)
        };

        // Act
        var result = exclusion.IsExpired(24);

        // Assert
        result.ShouldBeTrue();
    }
}