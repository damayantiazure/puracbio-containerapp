using Rabobank.Compliancy.Application.Interfaces.MonitoringDashboard;

namespace Rabobank.Compliancy.Application.MonitoringDashboard.Dto;
public class ComplianceScannerPrinciplesDashboardTileInformation : IMonitoringDashboardTileProcessInformation
{
    public string Title => "Compliancy Principles";

    public bool? GetSuccessCondition(long amount)
    {
        return amount >= 100;
    }
}
