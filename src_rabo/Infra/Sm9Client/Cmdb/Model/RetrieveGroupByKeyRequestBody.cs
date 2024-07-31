using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

public class RetrieveGroupByKeyRequestBody
{
    public IEnumerable<string>? Key { get; set; }
        
    /// <summary>
    /// Apparently this property is case sensitive. The others are not.
    /// </summary>
    [JsonProperty(PropertyName = "type")]
    public string? Type { get; set; }
    public string? Source { get; set; }
}