namespace Rabobank.Compliancy.Functions.ValidateGates.Model;

public enum ApprovalType
{ 
    NoApproval,
    PipelineApproval,
    PullRequestApproval
}
    
public class ValidateApproversResult
{
    public ApprovalType DeterminedApprovalType { get; set; }
    public string Message { get; set; }
}