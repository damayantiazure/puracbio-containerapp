using Microsoft.TeamFoundation.Build.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Build;

/// <summary>
/// Updates the project's retention settings.
/// https://learn.microsoft.com/en-us/rest/api/azure/devops/build/retention/update?view=azure-devops-rest-7.0
/// </summary>
public class SetBuildRetentionRequest : HttpPatchRequest<IDevHttpClientCallHandler, ProjectRetentionSetting, UpdateProjectRetentionSettingModel>
{
    private readonly string _organization;
    private readonly Guid _projectId;

    protected override string Url => $"{_organization}/{_projectId}/_apis/build/retention";

    protected override Dictionary<string, string> QueryStringParameters { get; } = new()
    {
        { "api-version", "7.0" }
    };

    public SetBuildRetentionRequest(string organization, Guid projectId, UpdateProjectRetentionSettingModel updateProjectRetentionSettingModel,
        IDevHttpClientCallHandler callHandler) : base(updateProjectRetentionSettingModel, callHandler)
    {
        _organization = organization;
        _projectId = projectId;
    }
}