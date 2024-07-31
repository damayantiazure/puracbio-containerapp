namespace Rabobank.Compliancy.Domain.Compliancy;

public class Approval
{
    public Guid Id { get; set; }

    public string Status { get; set; }

    public ApprovalStep[] Steps { get; set; }
}