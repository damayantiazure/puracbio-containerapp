using Rabobank.Compliancy.Clients.HttpClientExtensions;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;
using Rabobank.Compliancy.Clients.LogAnalyticsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.LogAnalyticsClient.Requests.Workspace.Models;

namespace Rabobank.Compliancy.Clients.LogAnalyticsClient.Requests.Workspace;

public class GetWorkspaceQueryRequest : HttpPostRequest<ILogAnalyticsCallHandler, WorkspaceResponse?, WorkspaceRequestBody>
{
    private readonly string _workspace;

    protected override string? Url => $"v1/workspaces/{_workspace}/query";

    protected override Dictionary<string, string> QueryStringParameters => new();

    public GetWorkspaceQueryRequest(string workspace, string kustoQuery, IHttpClientCallDistributor<ILogAnalyticsCallHandler> httpClientCallDistributor)
        : base(new WorkspaceRequestBody(kustoQuery), httpClientCallDistributor)
    {
        _workspace = workspace;
    }
}