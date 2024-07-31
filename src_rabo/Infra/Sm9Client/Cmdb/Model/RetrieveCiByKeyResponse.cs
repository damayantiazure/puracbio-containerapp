using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

public class RetrieveCiByKeyResponse
{
    public string[]? Messages { get; set; }

    public string? ReturnCode { get; set; }

    [JsonProperty(PropertyName = "retrieveCiInfoByKey")]
    public CiInfo? CiInfo { get; set; }
}