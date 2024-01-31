namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Model;

public class DeleteRegistrationRequest
{
    public string CiIdentifier { get; set; }
    public string Environment { get; set; }
    public string Profile { get; set; }
}