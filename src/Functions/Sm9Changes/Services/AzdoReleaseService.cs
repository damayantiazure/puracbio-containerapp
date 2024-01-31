#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Functions.Sm9Changes.Model;
using Rabobank.Compliancy.Functions.Sm9Changes.Extensions;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Services;

public class AzdoReleaseService : IAzdoService
{
    private readonly IAzdoRestClient _azdoClient;
    private readonly AzdoBuildService _sm9ChangesBuildService;

    public AzdoReleaseService(
        IAzdoRestClient azdoClient,
        AzdoBuildService sm9ChangesBuildService)
    {
        _azdoClient = azdoClient;
        _sm9ChangesBuildService = sm9ChangesBuildService;
    }

    public async Task<bool> IsLowRiskChangeAsync(string organization, string projectId, string runId)
    {
        var tags = await _azdoClient.GetAsync(ReleaseManagement.Tags(projectId, runId), organization);
        if (tags != null && tags.Value.Any(t => t.IsLowRiskChange()))
        {
            return true;
        }

        var changeId = await GetChangeIdVariableAsync(organization, projectId, runId);
        return changeId.IsLowRiskChange();
    }

    public async Task<string?> GetChangeIdFromVariableAsync(
        string organization, string projectId, string runId)
    {
        var changeId = await GetChangeIdVariableAsync(organization, projectId, runId);
        return changeId.IsValidChangeId()
            ? changeId
            : null;
    }

    private async Task<string> GetChangeIdVariableAsync(string organization, string projectId, string runId)
    {
        var classicRelease = await _azdoClient.GetAsync(ReleaseManagement.Release(projectId, runId), organization);
        return classicRelease?.Variables
            .FirstOrDefault(v => v.Key == SM9Constants.ChangeIdVarName).Value?.Value;
    }

    public async Task<IEnumerable<string>> GetChangeIdsFromTagsAsync(
        string organization, string projectId, string runId, string regex)
    {
        var tags = await _azdoClient.GetAsync(ReleaseManagement.Tags(projectId, runId), organization);
        return tags?.Value.GetChangeIdsFromTags(regex);
    }

    public async Task SetTagAsync(string organization, string projectId, string runId, string changeId, string urlHash)
    {
        var tags = await _azdoClient.GetAsync(ReleaseManagement.Tags(projectId, runId), organization);
        if (tags != null && tags.Value.Any(t => t == changeId))
        {
            await _azdoClient.DeleteAsync(ReleaseManagement.Tag(projectId, runId, changeId), organization);
        }

        await _azdoClient.PatchAsync(ReleaseManagement.Tag(projectId, runId, $"{changeId} [{urlHash}]"), null, organization);
    }

    public async Task<string> GetPipelineRunUrlAsync(string organization, string projectId, string runId) =>
        (await _azdoClient.GetAsync(ReleaseManagement.Release(projectId, runId), organization))?.Links.Web.Href.AbsoluteUri;

    public async Task<string> GetPipelineRunInitiatorAsync(string organization, string projectId, string runId)
    {
        var release = await _azdoClient.GetAsync(ReleaseManagement.Release(projectId, runId), organization);
        var initiator = release.CreatedBy?.UniqueName;
        if (initiator.IsValidEmail())
        {
            return initiator;
        }

        initiator = release.CreatedFor?.UniqueName;
        if (initiator.IsValidEmail())
        {
            return initiator;
        }

        var buildIds = release.Artifacts
            .Where(a => a.Type == "Build" &&
                        a.DefinitionReference.IsTriggeringArtifact != null &&
                        a.DefinitionReference.IsTriggeringArtifact.Id == "True" &&
                        a.DefinitionReference.IsTriggeringArtifact.Name == "True")
            .Select(a => a.DefinitionReference.Version.Id);
        foreach (var buildId in buildIds)
        {
            initiator = await _sm9ChangesBuildService.GetPipelineRunInitiatorAsync(organization, projectId, buildId);
            if (initiator.IsValidEmail())
            {
                return initiator;
            }
        }

        var pipeline =
            await _azdoClient.GetAsync(ReleaseManagement.Definition(projectId, release.ReleaseDefinition.Id), organization);
        initiator = pipeline.ModifiedBy?.UniqueName;
        if (initiator.IsValidEmail())
        {
            return initiator;
        }

        initiator = pipeline.CreatedBy?.UniqueName;
        if (initiator.IsValidEmail())
        {
            return initiator;
        }

        return default;
    }
}