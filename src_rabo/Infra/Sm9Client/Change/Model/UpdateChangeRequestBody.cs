using Rabobank.Compliancy.Infra.Sm9Client.Cmdb;

namespace Rabobank.Compliancy.Infra.Sm9Client.Change.Model;

public class UpdateChangeRequestBody
{
    [Newtonsoft.Json.JsonProperty("ChangeID")]
    public string ChangeId { get; set; }
    [Newtonsoft.Json.JsonProperty("approvalDetails")]
    public ApprovalDetails[]? ApprovalDetails { get; set; }
    public string? JournalUpdate { get; set; }
    public string Source { get; set; }

    public UpdateChangeRequestBody(string changeId)
    {
        ChangeId = changeId;
        Source = CmdbClient.AzdoCompliancyCiName;
    }
}