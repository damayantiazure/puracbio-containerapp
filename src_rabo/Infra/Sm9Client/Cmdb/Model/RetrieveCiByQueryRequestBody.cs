namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

public class RetrieveCiByQueryRequestBody
{
    public string? Type { get; set; }
    public string? Source { get; set; }
    public int? CountNum { get; set; }
    public int? StartNum { get; set; }
}
