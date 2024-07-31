using Rabobank.Compliancy.Application.Interfaces.MonitoringDashboard;

namespace Rabobank.Compliancy.Application.MonitoringDashboard.Dto;
public class Sm9ChangesErrorDashboardTileInformation : IMonitoringDashboardTileProcessInformation
{
    public string Title => "Sm9Changes errors";

    public bool? GetSuccessCondition(long amount)
    {
        return amount == 0;
    }
}
