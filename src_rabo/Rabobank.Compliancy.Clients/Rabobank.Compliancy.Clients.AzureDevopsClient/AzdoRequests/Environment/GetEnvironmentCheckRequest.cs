using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Environment.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Environment;

/// <summary>
///     Used to get CheckConfigurations by resourcetype and id from the URL
///     "{organization}/{project}/_apis/pipelines/checks/configurations?resourceType={resourceType}&resourceId
///     ={resourceId}".
///     This request requires the following permissions
///     - ?
/// </summary>
public class GetEnvironmentCheckRequest : HttpGetRequest<IDevHttpClientCallHandler,
    ResponseCollection<CheckConfiguration>>
{
    private readonly string _organization;
    private readonly Guid _projectId;
    private readonly string _resourceId;
    private readonly string _resourceType;

    public GetEnvironmentCheckRequest(string organization, Guid projectId, string resourceId, string resourceType,
        IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
        _resourceId = resourceId;
        _resourceType = resourceType;
    }

    protected override string Url => $"{_organization}/{_projectId}/_apis/pipelines/checks/configurations";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.1-preview" },
        { "resourceType", _resourceType },
        { "resourceId", _resourceId }
    };
}