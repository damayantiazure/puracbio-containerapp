using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

public class ManageDeploymentInformationBody
{
    public string? Key { get; set; }
    public string? Source { get; set; }
    public string? Type { get; set; }
    public string? Update { get; set; }
        
    [JsonProperty("deployment.information")]
    public IEnumerable<DeploymentInformation>? DeploymentInformations { get; set; }
}