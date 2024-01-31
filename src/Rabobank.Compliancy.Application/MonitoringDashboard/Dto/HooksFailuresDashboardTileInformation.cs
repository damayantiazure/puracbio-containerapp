using Rabobank.Compliancy.Application.Interfaces.MonitoringDashboard;

namespace Rabobank.Compliancy.Application.MonitoringDashboard.Dto;
public class HooksFailuresDashboardTileInformation : IMonitoringDashboardTileProcessInformation
{
    public string Title => "Hook failures";

    public bool? GetSuccessCondition(long amount)
    {
        return amount == 0;
    }
}
