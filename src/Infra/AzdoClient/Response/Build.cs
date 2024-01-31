using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class Build
{
    public Project Project { get; set; }
    public int Id { get; set; }
    public Definition Definition { get; set; }
    public string Result { get; set; }
    public string BuildNumber { get; set; }
    public DateTime QueueTime { get; set; }
    public DateTime StartTime { get; set; }
    public AgentQueue Queue { get; set; }
    public RequestedFor RequestedFor { get; set; }
    public IdentityRef RequestedBy { get; set; }
    public string SourceVersion { get; set; }
    public Repository Repository { get; set; }
    public IEnumerable<string> Tags { get; set; }
    [JsonProperty("_links")]
    public Links Links { get; set; }
    public TriggerInfo TriggerInfo { get; set; }
    public string Parameters { get; set; }
    public string SourceBranch { get; set; }
}