namespace Rabobank.Compliancy.Infra.Sm9Client.Change.Model;

public class UpdateChangeRequest
{
    [Newtonsoft.Json.JsonProperty(PropertyName = "updateChange")]
    public UpdateChangeRequestBody? UpdateChange { get; set; }
}