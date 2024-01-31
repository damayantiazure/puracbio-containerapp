using Rabobank.Compliancy.Application.Interfaces.MonitoringDashboard;

namespace Rabobank.Compliancy.Application.MonitoringDashboard.Dto;
public class ValidateGatesErrorDashboardTileInformation : IMonitoringDashboardTileProcessInformation
{
    public string Title => "ValidateGates errors";

    public bool? GetSuccessCondition(long amount)
    {
        return amount == 0;
    }
}
