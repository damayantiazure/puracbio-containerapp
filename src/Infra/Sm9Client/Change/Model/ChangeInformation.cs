namespace Rabobank.Compliancy.Infra.Sm9Client.Change.Model;

public class ChangeInformation
{
    public string[]? AffectedCI { get; set; }
    public string? ApprovalStatus { get; set; }
    public string? AssignedGroup { get; set; }
    public string? AssignedPerson { get; set; }
    public string? Category { get; set; }
    public string? ChangeId { get; set; }
    public string? ChangeModel { get; set; }
    public string? ChangeRequester { get; set; }
    public string? ClosureCode { get; set; }
    public string? Phase { get; set; }
    public string? Priority { get; set; }
    public string? RequestedEndDate { get; set; }
    public string? Status { get; set; }
    public string? Subcategory { get; set; }
    public string? Title { get; set; }
    public string? Url { get; set; }
    public bool HasCorrectPhase { get; set; }
}