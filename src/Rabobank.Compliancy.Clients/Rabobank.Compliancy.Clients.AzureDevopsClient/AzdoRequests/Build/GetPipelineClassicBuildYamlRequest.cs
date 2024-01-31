using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Build.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Build;

/// <summary>
/// Used to get the Yaml of a classic build pipeline using the URL {_organization}/{_projectId}/_apis/build/definitions/{_pipelineId}/yaml
/// </summary>
internal class GetPipelineClassicBuildYamlRequest : HttpGetRequest<IDevHttpClientCallHandler, PipelineClassicBuildYaml>
{
    private readonly string _organization;
    private readonly Guid _projectId;
    private readonly int _pipelineId;

    protected override string Url => $"{_organization}/{_projectId}/_apis/build/definitions/{_pipelineId}/yaml";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.0" }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPipelineClassicBuildYaml"/> class.
    /// </summary>        
    public GetPipelineClassicBuildYamlRequest(string organization, Guid projectId, int pipelineId, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
        _pipelineId = pipelineId;
    }
}