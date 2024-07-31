namespace Rabobank.Compliancy.Infra.Sm9Client.Change.Model;

public class CreateChangeResponse
{
    public string[]? Messages { get; set; }
    public string? ReturnCode { get; set; }
    [Newtonsoft.Json.JsonProperty(PropertyName = "createChange")]
    public ChangeData? ChangeData { get; set; }
}