using Rabobank.Compliancy.Application.Interfaces.MonitoringDashboard;

namespace Rabobank.Compliancy.Application.MonitoringDashboard.Dto;
public class AuditLoggingDashboardTileInformation : IMonitoringDashboardTileProcessInformation
{
    public string Title => "Audit Deployment";

    public bool? GetSuccessCondition(long amount)
    {
        return amount == 1;
    }
}
