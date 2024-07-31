namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class Approval
{
    public bool IsAutomated { get; set; }
    public IdentityRef Approver { get; set; }
}