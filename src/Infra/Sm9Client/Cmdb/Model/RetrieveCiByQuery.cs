namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

public class RetrieveCiByQuery
{
    public int More { get; set; }
    public IEnumerable<RetrieveCiByQueryResponseInformation>? Information { get; set; }
}
