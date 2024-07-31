using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Core.Approvals.Services;

public interface IClassicReleaseApproverService
{
    Task<bool> HasApprovalAsync(string projectId, string runId, string exclude, string organization = null);
    Task<IEnumerable<string>> GetAllApproversAsync(string projectId, string runId, string organization = null);
}