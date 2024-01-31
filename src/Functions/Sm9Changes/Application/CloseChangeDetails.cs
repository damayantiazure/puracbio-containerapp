namespace Rabobank.Compliancy.Functions.Sm9Changes.Application;

public class CloseChangeDetails
{
    [Newtonsoft.Json.JsonProperty("ChangeID")]
    public string ChangeId { get; set; }
    public string CompletionCode { get; set; }
    public string[] CompletionComments { get; set; }
}