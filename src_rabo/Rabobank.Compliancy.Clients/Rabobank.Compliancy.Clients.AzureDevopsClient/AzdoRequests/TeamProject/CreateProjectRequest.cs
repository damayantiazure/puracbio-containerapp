using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.TeamProject;

using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Operations;

/// <summary>
/// Used to create new Projects from the URL "{_organization}_apis/projects".
/// </summary>
public class CreateProjectRequest : HttpPostRequest<IDevHttpClientCallHandler, Operation, TeamProject>
{
    private readonly string _organization;
    private static readonly string Git = nameof(Git);

    private static Dictionary<string, Dictionary<string, string>> Capabilities => new()
    {
        {
            TeamProjectCapabilitiesConstants.VersionControlCapabilityName, new()
            {
                { TeamProjectCapabilitiesConstants.VersionControlCapabilityAttributeName, Git }
            }
        },
        {
            TeamProjectCapabilitiesConstants.ProcessTemplateCapabilityName, new()
            {
                { TeamProjectCapabilitiesConstants.ProcessTemplateCapabilityTemplateTypeIdAttributeName, ProcessTemplateTypeIdentifiers.VisualStudioScrum.ToString() }
            }
        }
    };

    protected override string Url => $"{_organization}/_apis/projects";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.0" }
    };

    public CreateProjectRequest(string organization, string projectName, string description
        , IDevHttpClientCallHandler httpClientCallHandler)
        : base(CreateRequestBody(projectName, description)
            , httpClientCallHandler)
    {
        _organization = organization;
    }

    private static TeamProject CreateRequestBody(string projectName, string description)
        => new()
        {
            Name = projectName,
            Description = description,
            Capabilities = Capabilities
        };
}