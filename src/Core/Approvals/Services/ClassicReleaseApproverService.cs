using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rabobank.Compliancy.Core.Approvals.Utils;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.AzdoClient.Response;

namespace Rabobank.Compliancy.Core.Approvals.Services;

public class ClassicReleaseApproverService : IClassicReleaseApproverService
{
    private readonly IAzdoRestClient _client;

    public ClassicReleaseApproverService(IAzdoRestClient client) =>
        _client = client;

    public async Task<bool> HasApprovalAsync(string projectId, string runId, string exclude, string organization = null)
    {
        var approvals = await GetApprovalsAsync(projectId, runId, organization);
        return approvals.Any(a => IsValidApprover(a, exclude));
    }

    public async Task<IEnumerable<string>> GetAllApproversAsync(string projectId, string runId, string organization = null)
    {
        var approvals = await GetApprovalsAsync(projectId, runId, organization);
        return approvals
            .Where(IsValidApprover)
            .OrderByDescending(a => a.ModifiedOn)
            .Select(a => a.ApprovedBy.UniqueName)
            .Distinct();
    }

    private Task<IEnumerable<ReleaseApproval>> GetApprovalsAsync(string projectId, string runId, string organization) =>
        _client.GetAsync(ReleaseManagement.Approvals(projectId, runId, "approved"), organization);

    private static bool IsValidApprover(ReleaseApproval approval, string exclude) =>
        !approval.IsAutomated && approval.ApprovedBy != null && approval.ApprovedBy.Id.ToString() != exclude
        && MailChecker.IsValidEmail(approval.ApprovedBy.UniqueName);

    private static bool IsValidApprover(ReleaseApproval approval) =>
        IsValidApprover(approval, null);
}