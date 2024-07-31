using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

public class RetrieveCiByKeyRequest
{
    [JsonProperty(PropertyName = "retrieveCiInfoByKey")]
    public RetrieveCiByKeyRequestBody? Body { get; set; }
}