using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

[ExcludeFromCodeCoverage]
public class GetCiResponse
{
    [JsonProperty(PropertyName = "content")]
    public IEnumerable<CiContentItem>? Content { get; set; }
}