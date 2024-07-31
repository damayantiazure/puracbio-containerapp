#nullable enable

using Flurl.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Rabobank.Compliancy.Core.Approvals.Services;
using Rabobank.Compliancy.Core.InputValidation.Model;
using Rabobank.Compliancy.Functions.ValidateGates.Model;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Extensions;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using System.Threading.Tasks;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;

namespace Rabobank.Compliancy.Functions.ValidateGates.Activities;

public class ValidateYamlApproversActivity
{
    private readonly IAzdoRestClient _azdoClient;
    private readonly IYamlReleaseApproverService _yamlReleaseApproversService;
    private readonly IPullRequestApproverService _pullRequestApprovalService;

    public ValidateYamlApproversActivity(IAzdoRestClient azdoClient,
        IYamlReleaseApproverService yamlReleaseApproversService,
        IPullRequestApproverService pullRequestApprovalService)
    {
        _azdoClient = azdoClient;
        _pullRequestApprovalService = pullRequestApprovalService;
        _yamlReleaseApproversService = yamlReleaseApproversService;
    }

    [FunctionName(nameof(ValidateYamlApproversActivity))]
    public async Task<ValidateApproversResult> RunAsync([ActivityTrigger]
        (string projectId, string runId, string organization) input)
    {
        try
        {
            if (await HasPullRequestApprovalAsync(input.projectId, input.runId, input.organization))
            {
                return new ValidateApproversResult
                    { DeterminedApprovalType = ApprovalType.PullRequestApproval, Message = "A pull request approval has been provided." };
            }

            if (await HasPipelineApprovalAsync(input.projectId, input.runId, input.organization))
            {
                return new ValidateApproversResult
                    { DeterminedApprovalType = ApprovalType.PipelineApproval, Message = "A pipeline approval has been provided." };
            }

            return new ValidateApproversResult
                { DeterminedApprovalType = ApprovalType.NoApproval, Message = ErrorMessages.CreateNoApprovalErrorMessage(ItemTypes.YamlPipeline) };
        }
        catch (FlurlHttpException ex)
        {
            throw await ex.MakeDurableFunctionCompatible();
        }

    }

    private async Task<bool> HasPullRequestApprovalAsync(string projectId, string runId, string organization) =>
        await _pullRequestApprovalService.HasApprovalAsync(projectId, runId, organization);

    private async Task<bool> HasPipelineApprovalAsync(string projectId, string runId, string organization)
    {
        var project = await _azdoClient.GetAsync(Project.ProjectById(projectId), organization);
        var yamlRelease = await _azdoClient.GetAsync(Builds.Build(projectId, runId), organization);
        return await _yamlReleaseApproversService.HasApprovalAsync(
            project, runId, yamlRelease.RequestedFor.UniqueName, organization);
    }
}