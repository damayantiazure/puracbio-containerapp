using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

public class ManageDeploymentInformationRequest
{
    [JsonProperty("managedeploymentinformation")]
    public ManageDeploymentInformationBody? Body { get; set; }
}