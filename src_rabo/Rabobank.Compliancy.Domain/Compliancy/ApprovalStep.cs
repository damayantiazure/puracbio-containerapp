using Rabobank.Compliancy.Domain.Enums;

namespace Rabobank.Compliancy.Domain.Compliancy;

public class ApprovalStep
{
    public string Status { get; set; }

    public Approver AssignedApprover { get; set; }
}