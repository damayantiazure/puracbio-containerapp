using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.ReleaseRequests;

/// <summary>
/// Used to get the Release Task Log (as a string) by task-id from the URL "{_organization}/{_projectId}/_apis/release/releases/{_releaseId}/environments/{_environmentId}/deployPhases/{_releaseDeployPhaseId}/tasks/{_taskId}/logs".
/// </summary>
public class GetReleaseTaskLogRequest : HttpGetRequest<IVsrmHttpClientCallHandler, string>
{
    private readonly string _organization;
    private readonly Guid _projectId;
    private readonly int _releaseId;
    private readonly int _environmentId;
    private readonly int _releaseDeployPhaseId;
    private readonly int _taskId;


    protected override string Url => $"{_organization}/{_projectId}/_apis/release/releases/{_releaseId}/environments/{_environmentId}/deployPhases/{_releaseDeployPhaseId}/tasks/{_taskId}/logs";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        {"api-version", "7.0"}
    };

    public GetReleaseTaskLogRequest(string organization, Guid projectId, int releaseId, int environmentId, int releaseDeployPhaseId, int taskId, IVsrmHttpClientCallHandler httpClientCallHandler) : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
        _releaseId = releaseId;
        _environmentId = environmentId;
        _releaseDeployPhaseId = releaseDeployPhaseId;
        _taskId = taskId;
    }
}