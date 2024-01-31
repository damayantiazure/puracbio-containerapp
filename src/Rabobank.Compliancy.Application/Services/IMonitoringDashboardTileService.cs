#nullable enable

namespace Rabobank.Compliancy.Application.Services;

/// <summary>
///     Service that allows collecting MonitoringDashboard information from LogAnalytics.
/// </summary>
public interface IMonitoringDashboardTileService
{
    /// <summary>
    /// Runs a query from a set of pre-determines queries.
    /// 
    /// The title parameter name can be derived from the title of the tile and should be one of the following:
    /// <br> - AuditLogging errors</br>
    /// <br> - Audit Deployment</br>
    /// <br> - AuditLogging poison messages</br>
    /// <br> - Compliancy CI's</br>
    /// <br> - Compliancy Pipelines</br>
    /// <br> - ComplScanOnline errors</br>
    /// <br> - Compliancy Principles</br>
    /// <br> - Compliancy Principles</br>
    /// <br> - Failed projects</br>
    /// <br> - Hook failures</br>
    /// <br> - Pipelinebreaker decorator error messages</br>
    /// <br> - PipelineBreaker errors</br>
    /// <br> - Sm9Changes errors</br>
    /// <br> - ValidateGates errors</br>
    /// </summary>
    /// <param name="title">The name of the query from the previous list</param>
    /// <param name="cancellationToken">Cancels the request if needed</param>
    /// <returns>As these are all "count" queries, used to represent a number on a tile, a number is returned to use on that tile.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the title parameter contains an unknown title</exception>
    Task<long> GetMonitoringDashboardDigitByTitle(string title, CancellationToken cancellationToken = default);
}