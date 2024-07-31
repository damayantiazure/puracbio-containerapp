namespace Rabobank.Compliancy.Clients.LogAnalyticsClient.Requests.Workspace.Models;

public class WorkspaceResponse
{
    public IEnumerable<WorkspaceTable> Tables { get; set; } = Enumerable.Empty<WorkspaceTable>();
}