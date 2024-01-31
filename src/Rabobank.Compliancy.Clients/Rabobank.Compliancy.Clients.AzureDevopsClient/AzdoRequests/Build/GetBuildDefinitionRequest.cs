using Rabobank.Compliancy.Clients.AzureDevopsClient.Helpers;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;
using Rabobank.Compliancy.Domain.Enums;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Build;

/// <summary>
/// Used to get BuildDefinitions by ID from the URL "{_organization}/{_projectId}/_apis/build/definitions/{_buildDefinitionId}".
/// 
/// This request requires the following permissions
///  - Pipeline
///    Administer build permissions
///    Delete build pipeline
///    Delete builds
///    Destroy builds
///    Edit build pipeline
///    Edit build quality
///    Manage build qualities
///    Manage build queue
///    Override check-in validation by build
///    Queue builds
///    Retain indefinitely
///    Stop builds
///    Update build information
///    View build pipeline
///    View builds
/// </summary>
public class GetBuildDefinitionRequest : HttpGetRequest<IDevHttpClientCallHandler, Microsoft.TeamFoundation.Build.WebApi.BuildDefinition>
{
    private readonly string _organization;
    private readonly Guid _projectId;
    private readonly int _buildDefinitionId;
    private readonly PipelineProcessType? _pipelineProcessType;

    protected override string Url => $"{_organization}/{_projectId}/_apis/build/definitions/{_buildDefinitionId}";

    protected override Dictionary<string, string> QueryStringParameters
    {
        get
        {
            var returnDict = new Dictionary<string, string>()
            {
                {"api-version", "7.0"}
            };

            if (_pipelineProcessType != null)
            {
                returnDict.Add("processType", _pipelineProcessType.Value.ToAzureDevopsInt().ToString());
            }

            return returnDict;
        }
    }

    public GetBuildDefinitionRequest(string organization, Guid projectId, int buildDefinitionId, PipelineProcessType? pipelineProcessType, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
        _buildDefinitionId = buildDefinitionId;
        _pipelineProcessType = pipelineProcessType;
    }

    public GetBuildDefinitionRequest(string organization, Guid projectId, int buildDefinitionId, IDevHttpClientCallHandler httpClientCallHandler)
        : this(organization, projectId, buildDefinitionId, null, httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
        _buildDefinitionId = buildDefinitionId;
    }
}