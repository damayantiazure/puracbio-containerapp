using Microsoft.Extensions.Configuration;
using Rabobank.Compliancy.Tests.Helpers;

namespace Rabobank.Compliancy.Infra.Sm9Client.Tests.Integration;

public class Sm9ClientConfiguration
{
    public Sm9ClientConfiguration()
    {
        Template = ConfigurationHelper.GetEnvironmentVariable("template");
        Requestor = ConfigurationHelper.GetEnvironmentVariable("requestor");
        Initiator = ConfigurationHelper.GetEnvironmentVariable("initiator");
        JournalUpdate = ConfigurationHelper.GetEnvironmentVariable("journalUpdate");
        Assets = new[] { ConfigurationHelper.GetEnvironmentVariable("assets") };
        Title = ConfigurationHelper.GetEnvironmentVariable("title");
        Description = ConfigurationHelper.GetEnvironmentVariable("description");
        ClosureCode = ConfigurationHelper.GetEnvironmentVariable("closureCode");
        ClosureComments = ConfigurationHelper.GetEnvironmentVariable("closureComments");
        ChangeId = ConfigurationHelper.GetEnvironmentVariable("changeId");
        CiName = ConfigurationHelper.GetEnvironmentVariable("CiName");
        Organization = ConfigurationHelper.GetEnvironmentVariable("Organization");
        Project = ConfigurationHelper.GetEnvironmentVariable("Project");
        Pipeline = ConfigurationHelper.GetEnvironmentVariable("Pipeline");
        Profile = ConfigurationHelper.GetEnvironmentVariable("Profile");
    }
    public string Template { get; set; }
    public string Requestor { get; set; }
    public string Initiator { get; set; }
    public string JournalUpdate { get; set; }
    public string[] Assets { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string ClosureCode { get; set; }
    public string ClosureComments { get; set; }
    public string ChangeId { get; set; }
    public string CiName { get; set; }
    public string Organization { get; set; }
    public string Project { get; set; }
    public string Profile { get; set; }
    public string Pipeline { get; set; }
}