using Rabobank.Compliancy.Application.Helpers;
using Rabobank.Compliancy.Application.Interfaces.MonitoringDashboard;
using Rabobank.Compliancy.Application.Services;

namespace Rabobank.Compliancy.Application.MonitoringDashboard;
public class MonitoringDashboardTileProcess : IMonitoringDashboardTileProcess
{
    protected readonly IMonitoringDashboardTileService _monitoringDashboardTileService;

    public MonitoringDashboardTileProcess(IMonitoringDashboardTileService monitoringDashboardService)
    {
        _monitoringDashboardTileService = monitoringDashboardService;
    }

    /// <inheritdoc />
    public async Task<string> GetWidgetContentForTile(IMonitoringDashboardTileProcessInformation tileProcessInformation, CancellationToken cancellationToken = default)
    {
        var amount = await _monitoringDashboardTileService.GetMonitoringDashboardDigitByTitle(tileProcessInformation.Title, cancellationToken);
        return WidgetFactory.CreateFoundRecordsWidgetContent(tileProcessInformation.Title, tileProcessInformation.GetSuccessCondition(amount), amount);
    }
}
