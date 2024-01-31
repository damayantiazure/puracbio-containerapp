using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.AuditLogging.Services;

public interface IMonitorDecoratorService
{
    Task MonitorDecoratorYamlReleaseAsync(string organization, string projectId, string runId, string stageName);
    Task MonitorDecoratorClassicReleaseAsync(string organization, string projectId, Release release, string stageName);
}