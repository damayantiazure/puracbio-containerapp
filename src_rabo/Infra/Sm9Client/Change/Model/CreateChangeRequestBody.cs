using Rabobank.Compliancy.Infra.Sm9Client.Cmdb;

namespace Rabobank.Compliancy.Infra.Sm9Client.Change.Model;

public class CreateChangeRequestBody
{
    public string Model { get; set; }
    public string Template { get; set; }
    public string? Requestor { get; set; }
    public string? Initiator { get; set; }
    public string[]? JournalUpdate { get; set; }
    public string[] Assets { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string Source { get; set; }

    public CreateChangeRequestBody(string template, string[] assets)
    {
        Template = template;
        Assets = assets;
        Model = CmdbClient.ChangeModel;
        Source = CmdbClient.AzdoCompliancyCiName;
    }
}