namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

public class CiInfo
{
    [Newtonsoft.Json.JsonProperty(PropertyName = "information")]
    public IEnumerable<ConfigurationItem>? ConfigurationItem { get; set; }
    public int More { get; set; }
}