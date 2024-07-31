using Rabobank.Compliancy.Application.Interfaces.MonitoringDashboard;

namespace Rabobank.Compliancy.Application.MonitoringDashboard.Dto;
public class PipelineBreakerErrorDashboardTileInformation : IMonitoringDashboardTileProcessInformation
{
    public string Title => "PipelineBreaker errors";

    public bool? GetSuccessCondition(long amount)
    {
        return null;
    }
}
