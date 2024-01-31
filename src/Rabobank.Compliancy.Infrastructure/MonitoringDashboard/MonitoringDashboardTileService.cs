#nullable enable

using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Monitoring;

namespace Rabobank.Compliancy.Infrastructure.MonitoringDashboard;

/// <inheritdoc />
public class MonitoringDashboardTileService : IMonitoringDashboardTileService
{
    private readonly ILogQueryService _logQueryService;
    private readonly Dictionary<string, string> _monitoringDashboardQueries = new Dictionary<string, string>
    {
        { "AuditLogging errors", "audit_logging_error_log_CL | where TimeGenerated > ago(24h) | count" },
        { "Audit Deployment", "audit_deployment_log_CL | where TimeGenerated > ago(4h) | limit 1 | count" },
        { "AuditLogging poison messages", "audit_poison_messages_log_CL | where TimeGenerated > ago(24h) | count" },
        { "Compliancy CI's", "compliancy_cis_CL | where TimeGenerated > ago(1d) | limit 101 | count" },
        { "Compliancy Pipelines", "compliancy_pipelines_CL | where TimeGenerated > ago(1d)| limit 101 | count" },
        { "ComplScanOnline errors", "compliance_scanner_online_error_log_CL | where TimeGenerated > ago(24h) | count" },
        { "Compliancy Principles", "compliancy_principles_CL | where TimeGenerated > ago(1d) | limit 101 | count " },
        { "Compliancy Rules", "compliancy_rules_CL | where TimeGenerated > ago(1d) | limit 101 | count " },
        { "Failed projects", "error_handling_log_CL  | where TimeGenerated > ago(1d) | distinct ProjectId_g | count" },
        { "Hook failures", "audit_logging_hook_failure_log_CL | where TimeGenerated > ago(24h) | count" },
        { "Pipelinebreaker decorator error messages", "decorator_error_log_CL | where TimeGenerated > ago(24h) | count" },
        { "PipelineBreaker errors", "pipeline_breaker_error_log_CL | where TimeGenerated > ago(24h) | count" },
        { "Sm9Changes errors", "sm9_changes_error_log_CL | where TimeGenerated > ago(24h) | count" },
        { "ValidateGates errors", "validate_gates_error_log_CL | where TimeGenerated > ago(24h) and InnerExceptionType_s != 'OrchestrationSessionNotFoundException' | count" }
    };

    public MonitoringDashboardTileService(ILogQueryService logQueryService)
    {
        _logQueryService = logQueryService;
    }

    /// <inheritdoc />
    public async Task<long> GetMonitoringDashboardDigitByTitle(string tileTitle, CancellationToken cancellationToken = default)
    {
        if (!_monitoringDashboardQueries.ContainsKey(tileTitle)) { throw new InvalidOperationException(); }

        var queryResponse = await _logQueryService.GetQueryEntryAsync<ScalarCountResult>(_monitoringDashboardQueries[tileTitle], cancellationToken);

        return queryResponse?.Count ?? 0;
    }
}