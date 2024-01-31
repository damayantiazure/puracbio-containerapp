namespace Rabobank.Compliancy.Infra.Sm9Client.Change.Model;

public class CreateChangeRequest
{
    [Newtonsoft.Json.JsonProperty(PropertyName = "createChange")]
    public CreateChangeRequestBody? Body { get; set; }
}