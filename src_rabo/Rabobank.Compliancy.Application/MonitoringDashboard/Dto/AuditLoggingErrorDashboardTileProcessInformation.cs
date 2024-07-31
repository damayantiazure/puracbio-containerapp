using Rabobank.Compliancy.Application.Interfaces.MonitoringDashboard;

namespace Rabobank.Compliancy.Application.MonitoringDashboard.Dto;
public class AuditLoggingErrorDashboardTileInformation : IMonitoringDashboardTileProcessInformation
{
    public string Title => "AuditLogging errors";

    public bool? GetSuccessCondition(long amount)
    {
        return amount == 0;
    }
}
