using System.Net.Http;
using System.Threading.Tasks;
using Rabobank.Compliancy.Domain.Compliancy.Authorizations;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;

public interface IAuthorizationService
{
    Task<bool> HasEditPermissionsAsync(HttpRequestMessage request, string organization, string projectId,
        string pipelineId, string pipelineType);

    Task<User> GetInteractiveUserAsync(HttpRequestMessage request, string organization);
}