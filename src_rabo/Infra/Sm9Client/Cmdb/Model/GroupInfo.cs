namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

public class GroupInfo
{
    [Newtonsoft.Json.JsonProperty(PropertyName = "information")]
    public IEnumerable<AssignmentGroup>? AssignmentGroup { get; set; }
}