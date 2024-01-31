namespace Rabobank.Compliancy.Infra.Sm9Client.Change.Model;

public class GetChangeByKeyRequest
{
    [Newtonsoft.Json.JsonProperty(PropertyName = "retrieveChangeInfoByKey")]
    public GetChangeByKeyRequestBody? Body { get; set; }
}