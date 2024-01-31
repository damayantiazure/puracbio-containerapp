using Rabobank.Compliancy.Application.Interfaces.MonitoringDashboard;

namespace Rabobank.Compliancy.Application.MonitoringDashboard.Dto;
public class ComplianceScannerRulesDashboardTileInformation : IMonitoringDashboardTileProcessInformation
{
    public string Title => "Compliancy Rules";

    public bool? GetSuccessCondition(long amount)
    {
        return amount >= 100;
    }
}
