using Rabobank.Compliancy.Application.Interfaces.MonitoringDashboard;

namespace Rabobank.Compliancy.Application.MonitoringDashboard.Dto;
public class AuditLoggingPoisonQueueDashboardTileInformation : IMonitoringDashboardTileProcessInformation
{
    public string Title => "AuditLogging poison messages";

    public bool? GetSuccessCondition(long amount)
    {
        return amount == 0;
    }
}
