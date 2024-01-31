using System.Collections.Generic;
using System.Threading.Tasks;
using Rabobank.Compliancy.Infra.AzdoClient.Response;

namespace Rabobank.Compliancy.Core.Approvals.Services;

public interface IYamlReleaseApproverService
{
    Task<bool> HasApprovalAsync(Project project, string runId, string exclude, string organization = null);
    Task<IEnumerable<string>> GetAllApproversAsync(Project project, string runId, string organization = null);
}