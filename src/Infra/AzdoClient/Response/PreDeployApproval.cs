namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class PreDeployApproval
{
    public string Status { get; set; }
    public string ApprovalType { get; set; }
    public bool IsAutomated { get; set; }
    public IdentityRef ApprovedBy { get; set; }
}