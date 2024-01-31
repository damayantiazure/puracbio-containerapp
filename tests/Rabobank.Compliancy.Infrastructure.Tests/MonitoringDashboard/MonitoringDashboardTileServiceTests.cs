using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Monitoring;
using Rabobank.Compliancy.Infrastructure.MonitoringDashboard;

namespace Rabobank.Compliancy.Infrastructure.Tests;

public class MonitoringDashboardTileServiceTests
{
    private readonly Mock<ILogQueryService> _logsQueryService = new();
    private readonly MonitoringDashboardTileService _sut;

    public MonitoringDashboardTileServiceTests() =>
        _sut = new MonitoringDashboardTileService(_logsQueryService.Object);

    [Theory]
    [InlineData("AuditLogging errors", "audit_logging_error_log_CL | where TimeGenerated > ago(24h) | count")]
    [InlineData("Audit Deployment", "audit_deployment_log_CL | where TimeGenerated > ago(4h) | limit 1 | count")]
    [InlineData("AuditLogging poison messages", "audit_poison_messages_log_CL | where TimeGenerated > ago(24h) | count")]
    [InlineData("Compliancy CI's", "compliancy_cis_CL | where TimeGenerated > ago(1d) | limit 101 | count")]
    [InlineData("Compliancy Pipelines", "compliancy_pipelines_CL | where TimeGenerated > ago(1d)| limit 101 | count")]
    [InlineData("ComplScanOnline errors", "compliance_scanner_online_error_log_CL | where TimeGenerated > ago(24h) | count")]
    [InlineData("Compliancy Principles", "compliancy_principles_CL | where TimeGenerated > ago(1d) | limit 101 | count ")]
    [InlineData("Compliancy Rules", "compliancy_rules_CL | where TimeGenerated > ago(1d) | limit 101 | count ")]
    [InlineData("Failed projects", "error_handling_log_CL  | where TimeGenerated > ago(1d) | distinct ProjectId_g | count")]
    [InlineData("Hook failures", "audit_logging_hook_failure_log_CL | where TimeGenerated > ago(24h) | count")]
    [InlineData("Pipelinebreaker decorator error messages", "decorator_error_log_CL | where TimeGenerated > ago(24h) | count")]
    [InlineData("PipelineBreaker errors", "pipeline_breaker_error_log_CL | where TimeGenerated > ago(24h) | count")]
    [InlineData("Sm9Changes errors", "sm9_changes_error_log_CL | where TimeGenerated > ago(24h) | count")]
    [InlineData("ValidateGates errors", "validate_gates_error_log_CL | where TimeGenerated > ago(24h) and InnerExceptionType_s != 'OrchestrationSessionNotFoundException' | count")]
    public async Task GetMonitoringDashboardDigitByTitle_WithDifferentTitles_ShouldReturnExpectedResponse(string title, string expectedQuery)
    {
        // Arrange
        _logsQueryService.Setup(logsQueryService => logsQueryService.GetQueryEntryAsync<ScalarCountResult>(expectedQuery, default))
            .ReturnsAsync(new ScalarCountResult() { Count = 1 })
            .Verifiable();

        // Act
        var actual = await _sut.GetMonitoringDashboardDigitByTitle(title);

        // Assert
        actual.Should().Be(1);
        _logsQueryService.Verify();
    }

    [Fact]
    public async Task GetMonitoringDashboardDigitByTitle_WithNullResponseFromQueryService_ShouldReturnZero()
    {
        // Arrange

        // Act
        var actual = await _sut.GetMonitoringDashboardDigitByTitle("AuditLogging errors");

        // Assert
        actual.Should().Be(0);
    }

    [Fact]
    public async Task GetMonitoringDashboardDigitByTitle_WithUnknownTitle_ShouldThrowInvalidOperationException()
    {
        // Arrange

        // Act
        var actual = () => _sut.GetMonitoringDashboardDigitByTitle("unknown");

        // Assert
        await actual.Should().ThrowAsync<InvalidOperationException>();
    }

}