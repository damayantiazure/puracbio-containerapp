namespace Rabobank.Compliancy.Infra.Sm9Client.Change.Model;

public class ChangeByKey
{
    public string[]? Key { get; set; }
    public string? Source { get; set; }
    public string? Type { get; set; }
    public ChangeInformation[]? Information { get; set; }
}