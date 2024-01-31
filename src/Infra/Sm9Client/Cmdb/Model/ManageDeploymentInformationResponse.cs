using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

public class ManageDeploymentInformationResponse
{
    public string[]? Messages { get; set; }
    public string? ReturnCode { get; set; }

    [JsonProperty("managedeploymentinformation")]
    public ManageDeploymentInformationDeploymentInfo? ManageDeploymentInformation { get; set; }
}