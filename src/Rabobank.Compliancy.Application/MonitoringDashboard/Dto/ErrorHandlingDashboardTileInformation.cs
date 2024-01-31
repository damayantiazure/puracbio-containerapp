using Rabobank.Compliancy.Application.Interfaces.MonitoringDashboard;

namespace Rabobank.Compliancy.Application.MonitoringDashboard.Dto;
public class ErrorHandlingDashboardTileInformation : IMonitoringDashboardTileProcessInformation
{
    public string Title => "Failed projects";

    public bool? GetSuccessCondition(long amount)
    {
        return null;
    }
}
