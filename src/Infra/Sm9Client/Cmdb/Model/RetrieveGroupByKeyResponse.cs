using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

public class RetrieveGroupByKeyResponse
{
    public string[]? Messages { get; set; }

    public string? ReturnCode { get; set; }

    [JsonProperty(PropertyName = "retrieveGroupInfoByKey")]
    public GroupInfo? GroupInfo { get; set; }
}