namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

public class RetrieveCiByKeyRequestBody
{
    public IEnumerable<string>? Key { get; set; }
    public string? RequestType { get; set; }
    public string? Source { get; set; }
}