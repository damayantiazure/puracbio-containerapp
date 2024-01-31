using Rabobank.Compliancy.Application.Interfaces.MonitoringDashboard;

namespace Rabobank.Compliancy.Application.MonitoringDashboard.Dto;
public class ComplianceScannerItemsDashboardTileInformation : IMonitoringDashboardTileProcessInformation
{
    public string Title => "Compliancy Pipelines";

    public bool? GetSuccessCondition(long amount)
    {
        return amount >= 100;
    }
}
