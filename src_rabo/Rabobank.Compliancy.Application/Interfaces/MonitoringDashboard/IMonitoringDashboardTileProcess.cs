namespace Rabobank.Compliancy.Application.Interfaces.MonitoringDashboard;

public interface IMonitoringDashboardTileProcess
{
    /// <summary>
    /// Gets the widget information for the Monitoring Dashboard tile.
    /// </summary>
    /// <param name="tileProcessInformation">The information needed to run this process containing the title of the tile and the condition of its colors</param>
    /// <param name="cancellationToken">Cancels the request if needed</param>
    /// <returns>Returns a string that contain all information for a widget tile on the dashboard</returns>
    Task<string> GetWidgetContentForTile(IMonitoringDashboardTileProcessInformation tileProcessInformation, CancellationToken cancellationToken = default);
}