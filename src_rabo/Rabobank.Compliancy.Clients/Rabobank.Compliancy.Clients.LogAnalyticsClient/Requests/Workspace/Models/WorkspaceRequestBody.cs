using Newtonsoft.Json;

namespace Rabobank.Compliancy.Clients.LogAnalyticsClient.Requests.Workspace.Models;

public class WorkspaceRequestBody
{
    public WorkspaceRequestBody(string query)
        => Query = query;

    [JsonProperty("query")]
    public string Query { get; }
}