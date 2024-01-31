#nullable enable

using Rabobank.Compliancy.Core.Approvals.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Services;

public class AzdoReleaseApproverService : IAzdoApproverService
{
    private readonly IAzdoRestClient _azdoClient;
    private readonly IClassicReleaseApproverService _pipelineApproverService;
    private readonly IPullRequestApproverService _pullRequestApproverService;

    public AzdoReleaseApproverService(
        IAzdoRestClient azdoClient,
        IClassicReleaseApproverService pipelineApproverService,
        IPullRequestApproverService pullRequestApproverService)
    {
        _azdoClient = azdoClient;
        _pipelineApproverService = pipelineApproverService;
        _pullRequestApproverService = pullRequestApproverService;
    }

    public async Task<(IEnumerable<string>, IEnumerable<string>)> GetApproversAsync(
        string organization, string projectId, string runId)
    {
        var pipelineApprovers = await _pipelineApproverService.GetAllApproversAsync(projectId, runId, organization);
        var pullRequestApprovers = await GetPullRequestApproversAsync(organization, projectId, runId);
        return (pipelineApprovers, pullRequestApprovers);
    }

    private async Task<IEnumerable<string>> GetPullRequestApproversAsync(string organization, string projectId, string releaseId)
    {
        var release = await _azdoClient.GetAsync(ReleaseManagement.Release(projectId, releaseId), organization);
            
        var buildArtifacts = GetBuildArtifacts(release);

        if (buildArtifacts == null || !buildArtifacts.Any())
        {
            return Enumerable.Empty<string>();
        }

        return (await Task.WhenAll(buildArtifacts
                .Select(async b => await _pullRequestApproverService.GetAllApproversAsync(b.projectId, b.buildId, organization))))
            .SelectMany(a => a)
            .Distinct();
    }

    private static IList<(string buildId, string projectId)>? GetBuildArtifacts(Release? release)
    {
        return release?.Artifacts?
            .Where(a => a.Type == "Build")
            .Select(a => (a.DefinitionReference.Version.Id, a.DefinitionReference.Project.Id))
            .ToList();
    }
}