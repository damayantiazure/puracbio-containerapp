using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Build;

/// <summary>
/// Gets a build by using the build identifier.
/// https://learn.microsoft.com/en-us/rest/api/azure/devops/build/builds/get?view=azure-devops-rest-7.0
/// </summary>
public class GetBuildRequest : HttpGetRequest<IDevHttpClientCallHandler, ResponseCollection<Microsoft.TeamFoundation.Build.WebApi.Build>>
{
    private readonly string _organization;
    private readonly Guid _projectId;
    private readonly int _buildId;

    protected override string Url => $"{_organization}/{_projectId}/_apis/build/builds/{_buildId}";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.0" }
    };

    public GetBuildRequest(string organization, Guid projectId, int buildId, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
        _buildId = buildId;
    }
}