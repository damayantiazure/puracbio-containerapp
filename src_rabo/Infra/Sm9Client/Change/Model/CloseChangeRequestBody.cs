using Rabobank.Compliancy.Infra.Sm9Client.Cmdb;

namespace Rabobank.Compliancy.Infra.Sm9Client.Change.Model;

public class CloseChangeRequestBody
{
    [Newtonsoft.Json.JsonProperty("ChangeID")]
    public string ChangeId { get; set; }
    public string? ClosureCode { get; set; }
    public string? ClosureComments { get; set; }
    public string Source { get; set; }

    public CloseChangeRequestBody(string changeId)
    {
        ChangeId = changeId;
        Source = CmdbClient.AzdoCompliancyCiName;
    }
}