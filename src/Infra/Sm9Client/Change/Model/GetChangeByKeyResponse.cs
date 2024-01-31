namespace Rabobank.Compliancy.Infra.Sm9Client.Change.Model;

public class GetChangeByKeyResponse
{
    public string[]? Messages { get; set; }
    public string? ReturnCode { get; set; }
    [Newtonsoft.Json.JsonProperty(PropertyName = "retrieveChangeInfoByKey")]
    public ChangeByKey? RetrieveChangeInfoByKey { get; set; }
}