using Rabobank.Compliancy.Core.Approvals.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Services;

public class AzdoBuildApproverService : IAzdoApproverService
{
    private readonly IAzdoRestClient _azdoClient;
    private readonly IYamlReleaseApproverService _pipelineApproverService;
    private readonly IPullRequestApproverService _pullRequestApproverService;

    public AzdoBuildApproverService(
        IAzdoRestClient azdoClient,
        IYamlReleaseApproverService pipelineApproverService,
        IPullRequestApproverService pullRequestApproverService)
    {
        _azdoClient = azdoClient;
        _pipelineApproverService = pipelineApproverService;
        _pullRequestApproverService = pullRequestApproverService;
    }

    public async Task<(IEnumerable<string>, IEnumerable<string>)> GetApproversAsync(
        string organization, string projectId, string runId)
    {
        var project = await _azdoClient.GetAsync(Project.ProjectById(projectId), organization);
        var pipelineApprovers = await _pipelineApproverService.GetAllApproversAsync(project, runId, organization);
        var pullRequestApprovers = await _pullRequestApproverService.GetAllApproversAsync(projectId, runId, organization);
        return (pipelineApprovers, pullRequestApprovers);
    }
}