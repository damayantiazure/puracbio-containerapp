using Rabobank.Compliancy.Infra.Sm9Client.Cmdb;

namespace Rabobank.Compliancy.Infra.Sm9Client.Change.Model;

public class GetChangeByKeyRequestBody
{
    public string Key { get; set; }
    public string Type { get; }
    public string Source { get; set; }

    public GetChangeByKeyRequestBody(string changeId)
    {
        Key = changeId;
        Type = "changeid";
        Source = CmdbClient.AzdoCompliancyCiName;
    }
}