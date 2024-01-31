using Rabobank.Compliancy.Application.Interfaces.MonitoringDashboard;

namespace Rabobank.Compliancy.Application.MonitoringDashboard.Dto;
public class ComplianceScannerOnlineErrorDashboardTileInformation : IMonitoringDashboardTileProcessInformation
{
    public string Title => "ComplScanOnline errors";

    public bool? GetSuccessCondition(long amount)
    {
        return null;
    }
}
