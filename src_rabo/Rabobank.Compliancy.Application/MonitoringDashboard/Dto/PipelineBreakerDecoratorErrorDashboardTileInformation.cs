using Rabobank.Compliancy.Application.Interfaces.MonitoringDashboard;

namespace Rabobank.Compliancy.Application.MonitoringDashboard.Dto;
public class PipelineBreakerDecoratorErrorDashboardTileInformation : IMonitoringDashboardTileProcessInformation
{
    public string Title => "Pipelinebreaker decorator error messages";

    public bool? GetSuccessCondition(long amount)
    {
        return amount == 0;
    }
}
