using Rabobank.Compliancy.Application.Interfaces.MonitoringDashboard;

namespace Rabobank.Compliancy.Application.MonitoringDashboard.Dto;
public class CompliancyScannerCisDashboardTileInformation : IMonitoringDashboardTileProcessInformation
{
    public string Title => "Compliancy CI's";

    public bool? GetSuccessCondition(long amount)
    {
        return amount >= 100;
    }
}
