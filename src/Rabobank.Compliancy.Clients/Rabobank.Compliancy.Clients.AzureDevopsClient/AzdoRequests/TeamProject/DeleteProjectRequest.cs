using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.TeamProject;

using Microsoft.VisualStudio.Services.Operations;

/// <summary>
/// Used to delete Projects by Id from the URL "{_organization}_apis/projects/{_projectId}".
/// </summary>
public class DeleteProjectRequest : HttpDeleteRequest<IDevHttpClientCallHandler, Operation>
{
    private readonly string _organization;
    private readonly Guid _projectId;

    protected override string Url => $"{_organization}/_apis/projects/{_projectId}";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.0" },
    };

    public DeleteProjectRequest(string organization, Guid projectId, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
    }
}