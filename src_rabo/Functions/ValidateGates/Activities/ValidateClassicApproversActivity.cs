#nullable enable

using Flurl.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Rabobank.Compliancy.Core.Approvals.Services;
using Rabobank.Compliancy.Core.InputValidation.Model;
using Rabobank.Compliancy.Functions.ValidateGates.Model;
using Rabobank.Compliancy.Infra.AzdoClient.Extensions;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Linq;
using System.Threading.Tasks;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.ValidateGates.Activities;

public class ValidateClassicApproversActivity
{
    private readonly IPullRequestApproverService _pullRequestApprovalService;
    private readonly IClassicReleaseApproverService _classicReleaseApprovalService;

    public ValidateClassicApproversActivity(IPullRequestApproverService pullRequestApprovalService,
        IClassicReleaseApproverService classicReleaseApprovalService)
    {
        _pullRequestApprovalService = pullRequestApprovalService;
        _classicReleaseApprovalService = classicReleaseApprovalService;
    }

    [FunctionName(nameof(ValidateClassicApproversActivity))]
    public async Task<ValidateApproversResult> RunAsync([ActivityTrigger]
        (string projectId, Release release, string organization) input)
    {
        try
        {
            if (await HasPipelineApprovalAsync(input.projectId, input.release, input.organization))
            {
                return new ValidateApproversResult
                    { DeterminedApprovalType = ApprovalType.PipelineApproval, Message = "A pipeline approval has been provided." };
            }

            if (await HasPullRequestApprovalAsync(input.projectId, input.release, input.organization))
            {
                return new ValidateApproversResult
                    { DeterminedApprovalType = ApprovalType.PullRequestApproval, Message = "A pull request approval has been provided." };
            }

            return new ValidateApproversResult
                { DeterminedApprovalType = ApprovalType.NoApproval, Message = ErrorMessages.CreateNoApprovalErrorMessage(ItemTypes.ClassicPipeline) };
        }
        catch (FlurlHttpException ex)
        {
            throw await ex.MakeDurableFunctionCompatible();
        }
    }
    private Task<bool> HasPipelineApprovalAsync(string projectId, Release release, string organization) =>
        _classicReleaseApprovalService.HasApprovalAsync(projectId, release.Id.ToString(), release.CreatedBy?.Id.ToString(), organization);

    private async Task<bool> HasPullRequestApprovalAsync(string projectId, Release release, string organization)
    {
        var buildIds = release.Artifacts?
            .Where(a => a.Type == "Build")
            .Select(a => a.DefinitionReference.Version.Id)
            .ToList();
        return buildIds != default && buildIds.Any() && (await Task.WhenAll(buildIds
                .Select(b => _pullRequestApprovalService.HasApprovalAsync(projectId, b, organization))))
            .All(a => a);
    }
}