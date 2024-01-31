using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

public class RetrieveCiByQueryResponse
{
    public string[]? Messages { get; set; }

    public string? ReturnCode { get; set; }

    [JsonProperty(PropertyName = "retrieveCiInfoByQuery")]
    public RetrieveCiByQuery? RetrieveCiByQuery { get; set; }
}
