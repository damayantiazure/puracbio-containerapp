namespace Rabobank.Compliancy.Clients.LogAnalyticsClient.Requests.Workspace.Models;

public class WorkspaceTable
{
    public string? Name { get; set; }
    public IList<WorkspaceColumn> Columns { get; set; } = new List<WorkspaceColumn>();
    public IList<IList<object?>> Rows { get; set; } = new List<IList<object?>>();
}