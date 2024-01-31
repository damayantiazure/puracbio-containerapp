using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

public class RetrieveGroupByKeyRequest
{
    [JsonProperty(PropertyName = "retrieveGroupInfoByKey")]
    public RetrieveGroupByKeyRequestBody? Body { get; set; }
}