#nullable enable

using Newtonsoft.Json.Linq;
using Rabobank.Compliancy.Functions.Sm9Changes.Extensions;
using Rabobank.Compliancy.Functions.Sm9Changes.Model;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Response = Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using System.Net;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Services;

public class AzdoBuildService : IAzdoService
{
    private readonly IAzdoRestClient _azdoClient;

    public AzdoBuildService(IAzdoRestClient azdoClient) => _azdoClient = azdoClient;

    public async Task<bool> IsLowRiskChangeAsync(string organization, string projectId, string runId)
    {
        var tags = await _azdoClient.GetAsync(Builds.Tags(projectId, runId), organization);
        if (tags != null &&  Array.Exists(tags.Value, t => t.IsLowRiskChange()))
        {
            return true;
        }

        var changeId = await GetChangeIdVariableAsync(organization, projectId, runId);
        return changeId.IsLowRiskChange();
    }

    public async Task<string?> GetChangeIdFromVariableAsync(string organization, string projectId, string runId)
    {
        var changeId = await GetChangeIdVariableAsync(organization, projectId, runId);
        return changeId.IsValidChangeId()
            ? changeId
            : null;
    }

    public async Task<IEnumerable<string>?> GetChangeIdsFromTagsAsync(
        string organization, string projectId, string runId, string regex)
    {
        var tags = await _azdoClient.GetAsync(Builds.Tags(projectId, runId), organization);
        return tags?.Value.GetChangeIdsFromTags(regex);
    }

    public async Task SetTagAsync(string organization, string projectId, string runId, string changeId, string urlHash)
    {
        var tags = await _azdoClient.GetAsync(Builds.Tags(projectId, runId), organization);
        if (tags != null && Array.Exists(tags.Value, t => t == changeId))
        {
            await _azdoClient.DeleteAsync(Builds.Tag(projectId, runId, changeId), organization);
        }

        await _azdoClient.PutAsync(Builds.Tag(projectId, runId, $"{changeId} [{urlHash}]"), null, organization);
    }

    public async Task<string?> GetPipelineRunUrlAsync(string organization, string projectId, string runId) =>
        (await _azdoClient.GetAsync(Builds.Build(projectId, runId), organization))?.Links.Web.Href.AbsoluteUri;

    public async Task<string?> GetPipelineRunInitiatorAsync(string organization, string projectId, string runId)
    {
        var build = await _azdoClient.GetAsync(Builds.Build(projectId, runId), organization);

        var initiator = build.RequestedBy?.UniqueName;
        if (initiator.IsValidEmail())
        {
            return initiator;
        }

        initiator = build.RequestedFor?.UniqueName;
        if (initiator.IsValidEmail())
        {
            return initiator;
        }

        if (build.TriggerInfo != null &&
            string.Equals(build.TriggerInfo.ArtifactType, "pipeline", StringComparison.InvariantCultureIgnoreCase))
        {
            initiator = await GetPipelineRunInitiatorAsync(organization, build.TriggerInfo.ProjectId, build.TriggerInfo.PipelineId);
            if (initiator.IsValidEmail())
            {
                return initiator;
            }
        }

        var changes = new List<Response.Change>();
        try
        {
            changes = (await _azdoClient.GetAsync(Builds.Changes(projectId, runId), organization)).ToList();
        }
        // Do not throw for Internal Server Error that occurs when a repository has been renamed or removed
        catch (FlurlHttpException e)
        {
            if (e.Call?.HttpStatus != HttpStatusCode.InternalServerError)
            {
                throw;
            }
        }

        foreach (var change in changes)
        {
            initiator = change.Author.UniqueName;
            if (initiator.IsValidEmail())
            {
                return initiator;
            }
        }

        var pipeline =
            await _azdoClient.GetAsync(Builds.BuildDefinition(projectId, build.Definition.Id), organization);
        initiator = pipeline.AuthoredBy?.UniqueName;
        return initiator.IsValidEmail() ? initiator : default;
    }

    private async Task<string?> GetChangeIdVariableAsync(string organization, string projectId, string runId)
    {
        var variables = (await _azdoClient.GetAsync(Builds.Build(projectId, runId), organization))?.Parameters;
        return variables == null
            ? null
            : (string?)JObject.Parse(variables).SelectToken(SM9Constants.ChangeIdVarName);
    }
}