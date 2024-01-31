namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;

[Obsolete("This class is part of an undocumented legacy API.")]
public class MembersGroupResponse
{
    public bool HasErrors { get; set; }
    public bool HasWarnings { get; set; }
    public IEnumerable<string>? AddedIdentities { get; set; }
    public IEnumerable<string>? FailedAddedIdentities { get; set; }
    public IEnumerable<string>? DeletedIdentities { get; set; }
    public IEnumerable<string>? FailedDeletedIdentities { get; set; }
    public IEnumerable<string>? GeneralErrors { get; set; }
    public IEnumerable<string>? LicenceErrors { get; set; }
    public IEnumerable<string>? StakeholderLicenceWarnings { get; set; }
    public IEnumerable<string>? AADErrors { get; set; }
}