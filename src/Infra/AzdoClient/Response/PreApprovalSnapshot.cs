using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class PreApprovalSnapshot
{
    public ApprovalOptions ApprovalOptions { get; set; }
    public IEnumerable<Approval> Approvals { get; set; }
}