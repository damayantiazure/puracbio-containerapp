namespace Rabobank.Compliancy.Infra.Sm9Client.Change.Model;

public class ApprovalDetails
{
    public string? ApprovalName { get; set; }
    public string? ApprovalComments { get; set; }

    public ApprovalDetails()
    {
    }

    public ApprovalDetails(string approvalName, string approvalComments)
    {
        ApprovalName = approvalName;
        ApprovalComments = approvalComments;
    }
}