using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

public class RetrieveCiByQueryResponseInformation
{
    public string? CiID { get; set; }
    public string? CiName { get; set; }
    public string? CiType { get; set; }
    public string? CiSubtype { get; set; }
    public string? CoreApplication { get; set; }
    public string? Status { get; set; }
    public string? ConfigAdminGroup { get; set; }
    [JsonProperty(PropertyName = "DeploymentInformation")]
    public IEnumerable<DeploymentInfo>? DeploymentInfo { get; set; }
    public string? AicClassification { get; set; }
    public IEnumerable<string>? Environment { get; set; }
    public string? SoxClassification { get; set; }
}