using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

public class RetrieveCiByQueryRequest
{
    [JsonProperty(PropertyName = "retrieveCiInfoByQuery")]
    public RetrieveCiByQueryRequestBody? Body { get; set; }
}
