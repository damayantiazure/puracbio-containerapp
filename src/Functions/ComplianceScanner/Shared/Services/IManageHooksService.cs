using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;

public interface IManageHooksService
{
    Task ManageHooksOrganizationAsync(string organization);
    Task CreateHookAsync(string organization, string projectId, string pipelineType, string pipelineId);
}