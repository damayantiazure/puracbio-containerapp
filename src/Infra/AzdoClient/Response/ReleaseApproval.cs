using System;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class ReleaseApproval
{
    public string Status { get; set; }
    public string ApprovalType { get; set; }
    public bool IsAutomated { get; set; }
    public IdentityRef ApprovedBy { get; set; }
    public DateTime ModifiedOn { get; set; }
}