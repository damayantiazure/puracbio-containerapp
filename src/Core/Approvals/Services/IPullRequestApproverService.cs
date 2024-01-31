using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Core.Approvals.Services;

public interface IPullRequestApproverService
{
    Task<bool> HasApprovalAsync(string projectId, string runId, string organization = null);
    Task<IEnumerable<string>> GetAllApproversAsync(string projectId, string runId, string organization = null);
}