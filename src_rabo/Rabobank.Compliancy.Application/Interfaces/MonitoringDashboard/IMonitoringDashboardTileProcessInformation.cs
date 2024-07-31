namespace Rabobank.Compliancy.Application.Interfaces.MonitoringDashboard;

public interface IMonitoringDashboardTileProcessInformation
{
    public string Title { get; }
    public bool? GetSuccessCondition(long amount);
}