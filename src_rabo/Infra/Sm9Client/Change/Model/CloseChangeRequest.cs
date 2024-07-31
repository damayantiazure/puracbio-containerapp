namespace Rabobank.Compliancy.Infra.Sm9Client.Change.Model;

public class CloseChangeRequest
{
    [Newtonsoft.Json.JsonProperty(PropertyName = "closeChange")]
    public CloseChangeRequestBody? Body { get; set; }
}