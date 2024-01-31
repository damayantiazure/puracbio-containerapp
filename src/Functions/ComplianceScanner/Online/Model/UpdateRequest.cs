namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Model;

public class UpdateRequest
{
    public string FieldToUpdate { get; set; }
    public string CiIdentifier { get; set; }
    public string Environment { get; set; }
    public string Profile { get; set; }
    public string NewValue { get; set; }
}