using Rabobank.Compliancy.Clients.AzureDevopsClient.Helpers;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;
using Rabobank.Compliancy.Domain.Enums;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Build;

/// <summary>
/// Used to get BuildDefinitions by project from the URL "{_organization}/{_projectId}/_apis/build/definitions".
/// 
/// Provides the option to filter by processtype and / or add "all properties".
///
/// This request requires the following permissions
///  - ?
/// </summary>
public class GetAllBuildDefinitionsForProjectRequest : HttpGetRequest<IDevHttpClientCallHandler, ResponseCollection<Microsoft.TeamFoundation.Build.WebApi.BuildDefinition>>
{
    private readonly string _organization;
    private readonly Guid _projectId;
    private readonly PipelineProcessType? _pipelineProcessType;
    private readonly bool _includeAllProperties;

    protected override string Url => $"{_organization}/{_projectId}/_apis/build/definitions";

    protected override Dictionary<string, string> QueryStringParameters
    {
        get
        {
            var returnDict = new Dictionary<string, string>()
            {
                {"api-version", "7.1-preview"},
                {"includeAllProperties", _includeAllProperties.ToString()}
            };

            if (_pipelineProcessType != null)
            {
                returnDict.Add("processType", _pipelineProcessType.Value.ToAzureDevopsInt().ToString());
            }

            return returnDict;
        }
    }

    public GetAllBuildDefinitionsForProjectRequest(string organization, Guid projectId, PipelineProcessType? pipelineProcessType, bool includeAllProperties, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
        _pipelineProcessType = pipelineProcessType;
        _includeAllProperties = includeAllProperties;
    }

    public GetAllBuildDefinitionsForProjectRequest(string organization, Guid projectId, bool includeCapabilities, IDevHttpClientCallHandler httpClientCallHandler)
        : this(organization, projectId, null, includeCapabilities, httpClientCallHandler)
    {
    }
}