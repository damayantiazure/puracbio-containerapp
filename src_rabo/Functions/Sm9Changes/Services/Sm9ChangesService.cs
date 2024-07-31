#nullable enable

using Rabobank.Compliancy.Functions.Sm9Changes.Model;
using Rabobank.Compliancy.Infra.Sm9Client.Change;
using Rabobank.Compliancy.Infra.Sm9Client.Change.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Services;

public class Sm9ChangesService : ISm9ChangesService
{
    private const int _validateChangeInterval = 10;

    private readonly IChangeClient _changeClient;

    public Sm9ChangesService(IChangeClient changeClient) => _changeClient = changeClient;

    public void ValidateFunctionInput(
        HttpRequestMessage request, [NotNull] string? organization, Guid projectId, [NotNull] string? pipelineType, int runId)
    {
        if (string.IsNullOrEmpty(organization))
        {
            throw new ArgumentException($"'{nameof(organization)}' is not provided in the request url");
        }

        if (projectId == Guid.Empty)
        {
            throw new ArgumentException($"'{nameof(projectId)}' is not provided in the request url");
        }

        if (string.IsNullOrEmpty(pipelineType))
        {
            throw new ArgumentException($"'{nameof(pipelineType)}' is not provided in the request url");
        }

        if (pipelineType != SM9Constants.BuildPipelineType && pipelineType != SM9Constants.ReleasePipelineType)
        {
            throw new ArgumentOutOfRangeException(nameof(pipelineType),
                $"An invalid '{nameof(pipelineType)}' has been provided in the request url." +
                $"{nameof(pipelineType)}: {pipelineType} is not supported");
        }

        if (runId < 1)
        {
            throw new ArgumentException($"'{nameof(runId)}' is not provided in the request url");
        }

        if (request.Content == null)
        {
            throw new ArgumentException($"'{nameof(request.Content)}' is not provided in the request message");
        }
    }

    public void ValidateFunctionInput(
        HttpRequestMessage request, [NotNull]string? organization, [NotNull] string? projectId, [NotNull] string? pipelineType, [NotNull] string? runId)
    {
        if (string.IsNullOrEmpty(organization))
        {
            throw new ArgumentNullException(nameof(organization),
                $"'{nameof(organization)}' is not provided in the request url");
        }

        if (string.IsNullOrEmpty(projectId))
        {
            throw new ArgumentNullException(nameof(projectId),
                $"'{nameof(projectId)}' is not provided in the request url");
        }

        if (string.IsNullOrEmpty(pipelineType))
        {
            throw new ArgumentNullException(nameof(pipelineType),
                $"'{nameof(pipelineType)}' is not provided in the request url");
        }

        if (pipelineType != SM9Constants.BuildPipelineType && pipelineType != SM9Constants.ReleasePipelineType)
        {
            throw new ArgumentOutOfRangeException(nameof(pipelineType),
                $"An invalid '{nameof(pipelineType)}' has been provided in the request url." +
                $"{nameof(pipelineType)}: {pipelineType} is not supported");
        }

        if (string.IsNullOrEmpty(runId))
        {
            throw new ArgumentNullException(nameof(runId), 
                $"'{nameof(runId)}' is not provided in the request url");
        }

        if (request.Content == null)
        {
            throw new ArgumentNullException(nameof(request),
                $"'{nameof(request.Content)}' is not provided in the request message");
        }
    }

    public async Task<IEnumerable<ChangeInformation>> ValidateChangesAsync(IEnumerable<string> changeIds,
        IEnumerable<string> correctChangePhases,
        int validateChangeTimeOut) =>
        await Task.WhenAll(changeIds
            .Select(async c => await ValidateChangeAsync(c, correctChangePhases, validateChangeTimeOut))
            .Select(s => s));

    private async Task<ChangeInformation> ValidateChangeAsync(string changeId, IEnumerable<string> correctChangePhases,
        int validateChangeTimeOut)
    {
        string? changePhase = null;
        var isCorrectPhase = false;
        var elapsedTime = 0;

        while (!isCorrectPhase && elapsedTime < validateChangeTimeOut)
        {
            changePhase = (await _changeClient.GetChangeByKeyAsync(
                    new GetChangeByKeyRequestBody(changeId)))?
                .RetrieveChangeInfoByKey?.Information?[0].Phase;
                    
            isCorrectPhase = correctChangePhases.Contains(changePhase, StringComparer.InvariantCultureIgnoreCase);
            if (isCorrectPhase) continue;
            Thread.Sleep(_validateChangeInterval * 500);
            elapsedTime += _validateChangeInterval;
        }            

        var changeDetails = new ChangeInformation
        {
            ChangeId = changeId,
            Phase = changePhase,
            HasCorrectPhase = isCorrectPhase
        };

        return changeDetails;
    }

    public async Task ApproveChangesAsync(string organization, IEnumerable<string> changeIds,
        IEnumerable<string> pipelineApprovers, IEnumerable<string> pullRequestApprovers)
    {
        var approvalDetails = pipelineApprovers
            .Select(approver => new ApprovalDetails(approver, "pipelineApprover"))
            .Concat(pullRequestApprovers
                .Select(approver => new ApprovalDetails(approver, "pullRequestApprover")))
            .ToArray();

        foreach (var changeId in changeIds)
        {
            var approvalHistory = new UpdateChangeRequestBody(changeId)
            {
                ApprovalDetails = approvalDetails                    
            };

            await ApproveChangeAsync(organization, approvalHistory);
        }
    }

    private async Task ApproveChangeAsync(string organization, UpdateChangeRequestBody approvalHistory)
    {
        try
        {
            await _changeClient.UpdateChangeAsync(approvalHistory);
        }

        // Dirty fix for account issue North America, where most .com accounts don't have access to SM9
        // It seems like the fix is not working properly. For now, ITSM/SM9 does not validate the approvals anymore so this would be obsolete
        // Details of the investigation on this issue can be found here (under investigation results):
        // https://dev.azure.com/raboweb/TAS/_sprints/taskboard/Aegis/TAS/22Q3/Sprint%20127?workitem=4969219
        catch (ChangeClientException firstEx)
        {
            if (!IsNorthAmericaIssue(organization, firstEx))
            {
                throw;
            }

            try
            {
                //Try approval with @raboag.com account
                approvalHistory = ApproveChangeBodyWithOtherAccount(
                    firstEx, approvalHistory, "@rabobank.com", "@raboag.com");
                await _changeClient.UpdateChangeAsync(approvalHistory);
            }
            catch (ChangeClientException secondEx)
            {
                if (!IsNorthAmericaIssue(organization, secondEx))
                {
                    throw;
                }

                //Try approval with @rabo.com account
                approvalHistory = ApproveChangeBodyWithOtherAccount(
                    secondEx, approvalHistory, "@raboag.com", "@rabo.com");
                await _changeClient.UpdateChangeAsync(approvalHistory);
            }
        }
    }

    private static bool IsNorthAmericaIssue(string organization, Exception e) =>
        organization == "raboweb-na" &&
        e.Message.Contains("Please make sure you input a correct operator");

    private static UpdateChangeRequestBody ApproveChangeBodyWithOtherAccount(Exception e,
        UpdateChangeRequestBody oldApprovalHistory, string oldEmailDomain, string newEmailDomain)
    {
        var invalidApprovers = Regex.Match(e.Message, @"Please make sure you input a correct operator: (.+?)""")
            .Groups[1].Value.Split(",");

        var approvalDetails = oldApprovalHistory.ApprovalDetails?
            .Where(x => invalidApprovers.Contains(x.ApprovalName))
            .Select(x => new ApprovalDetails()
            {
                ApprovalName = x.ApprovalName?
                    .Replace(oldEmailDomain, newEmailDomain, StringComparison.InvariantCultureIgnoreCase),
                ApprovalComments = x.ApprovalComments
            })
            .ToArray();

        return new UpdateChangeRequestBody(oldApprovalHistory.ChangeId)
        {
            ApprovalDetails = approvalDetails
        };
    }
}