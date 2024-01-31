#nullable enable

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.Approvals.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.Sm9Changes.Exceptions;
using Rabobank.Compliancy.Functions.Sm9Changes.Extensions;
using Rabobank.Compliancy.Functions.Sm9Changes.Helpers;
using Rabobank.Compliancy.Functions.Sm9Changes.Model;
using Rabobank.Compliancy.Functions.Sm9Changes.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.Sm9Client.Change;
using Rabobank.Compliancy.Infra.Sm9Client.Change.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ErrorMessages = Rabobank.Compliancy.Functions.Sm9Changes.Model.ErrorMessages;

namespace Rabobank.Compliancy.Functions.Sm9Changes;

public class ApproveChangeFunction
{
    private const int _validateChangeTimeOut = 1;
    private readonly Dictionary<string, IAzdoApproverService> _azdoApproverService;
    private readonly Dictionary<string, IAzdoService> _azdoService;

    private readonly IChangeClient _changeClient;
    private readonly ISm9ChangesService _sm9ChangesService;
    private readonly ILoggingService _loggingService;

    public ApproveChangeFunction(
        IAzdoRestClient azdoClient,
        IChangeClient changeClient,
        ILoggingService loggingService,
        ISm9ChangesService sm9ChangesService,
        IYamlReleaseApproverService yamlReleaseApproverService,
        IClassicReleaseApproverService classicReleaseApproverService,
        IPullRequestApproverService pullRequestApproverService)
    {
        _changeClient = changeClient;
        _loggingService = loggingService;
        _sm9ChangesService = sm9ChangesService;
        _azdoService = new Dictionary<string, IAzdoService>
        {
            [SM9Constants.BuildPipelineType] = new AzdoBuildService(azdoClient),
            [SM9Constants.ReleasePipelineType] = new AzdoReleaseService(azdoClient,
                new AzdoBuildService(azdoClient))
        };
        _azdoApproverService = new Dictionary<string, IAzdoApproverService>
        {
            [SM9Constants.BuildPipelineType] = new AzdoBuildApproverService(
                azdoClient, yamlReleaseApproverService, pullRequestApproverService),
            [SM9Constants.ReleasePipelineType] = new AzdoReleaseApproverService(
                azdoClient, classicReleaseApproverService, pullRequestApproverService)
        };
    }

    [FunctionName(nameof(ApproveChangeFunction))]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post",
            Route = "approve-change/{organization}/{projectId}/{pipelineType}/{runId}")]
        HttpRequestMessage request, string? organization, string? projectId, string? pipelineType, string? runId)
    {
        try
        {
            _sm9ChangesService.ValidateFunctionInput(request, organization, projectId, pipelineType, runId);

            var input = await GetUserInputAsync(request);
            var azdoService = _azdoService[pipelineType];
            var changeIds = await GetChangeIdsAsync(organization, projectId, runId, input, azdoService);

            var changeDetails = (await _sm9ChangesService.ValidateChangesAsync(
                    changeIds, new[] { SM9Constants.DeploymentPhase, SM9Constants.ExecutionPhase },
                    _validateChangeTimeOut)).ToList();

            var validChangeIds = changeDetails.Where(
                    changeInformation => changeInformation is { HasCorrectPhase: true, ChangeId: not null })
                .Select(changeInformation => changeInformation.ChangeId!).ToList();

            if (validChangeIds.Any())
            {
                var azdoApprovalService = _azdoApproverService[pipelineType];
                var (pipelineApprovers, pullRequestApprovers) =
                    await azdoApprovalService.GetApproversAsync(organization, projectId, runId);
                if (!pipelineApprovers.Any() && !pullRequestApprovers.Any())
                {
                    throw new ApproverNotFoundException(ErrorMessages.ApproverNotFound(null, false));
                }

                await _sm9ChangesService.ApproveChangesAsync(
                    organization, validChangeIds, pipelineApprovers, pullRequestApprovers);

                await UpdateChangesAsync(organization, projectId, runId, azdoService, validChangeIds);

                await Task.WhenAll(validChangeIds
                    .Select(async changeId => await SetTagAsync(organization, projectId, runId, changeId, azdoService)));
            }

            var invalidChanges = changeDetails.Where(changeInformation => !changeInformation.HasCorrectPhase);

            if (invalidChanges.Any())
            {
                throw new ChangePhaseValidationException(ErrorMessages.InvalidChangePhase(
                    invalidChanges, pipelineType, false));
            }

            return CreateResponseHelper.CreateResponseTask(ResponseMessages.SuccessfullyApproved(validChangeIds), true);
        }
        catch (ChangeClientException e)
        {
            var exceptionBaseMetaInformation = await LogExceptionAsync(e);
            return CreateResponseHelper.CreateResponseTask(ErrorMessages.Sm9TeamError(
                $"{e.Message} (CorrelationId:{exceptionBaseMetaInformation.CorrelationId})"), false);
        }
        catch (Exception e) when (
            e is InvalidUserInputException
                or ChangeIdNotFoundException
                or ChangePhaseValidationException
                or ApproverNotFoundException
                or PipelineUrlNotFoundException
                or ArgumentException)
        {
            var exceptionBaseMetaInformation = await LogExceptionAsync(e);
            return CreateResponseHelper.CreateResponseTask(
                $"{e.Message} (CorrelationId:{exceptionBaseMetaInformation.CorrelationId})", false);
        }
        catch (Exception e)
        {
            await LogExceptionAsync(e);
            throw;
        }

        async Task<ExceptionBaseMetaInformation> LogExceptionAsync(Exception e)
        {
            var exceptionBaseMetaInformation = new ExceptionBaseMetaInformation
                (request, nameof(ApproveChangeFunction), projectId)
            {
                Organization = organization,
                PipelineType = pipelineType,
                RunId = runId
            };

            await _loggingService.LogExceptionAsync(LogDestinations.Sm9ChangesErrorLog, exceptionBaseMetaInformation, e);
            return exceptionBaseMetaInformation;
        }
    }

    private static async Task<UpdateChangeRequestBody> GetUserInputAsync(HttpRequestMessage request)
    {
        if (request.Content == null)
        {
            throw new InvalidOperationException("Cannot retrieve content from request.");
        }

        var content = await request.Content.ReadAsStringAsync();
        var input = JsonConvert.DeserializeObject<UpdateChangeRequestBody>(content);
        if (input != null)
        {
            return input;
        }

        throw new InvalidOperationException("Cannot retrieve content from request.");
    }

    private static async Task<IEnumerable<string>> GetChangeIdsAsync(
        string organization, string projectId, string runId, UpdateChangeRequestBody input, IAzdoService azdoService)
    {
        var changeIds = input.ChangeId.IsValidChangeId()
            ? new[] { input.ChangeId }
            : (await azdoService.GetChangeIdsFromTagsAsync(
                organization, projectId, runId, SM9Constants.ChangeIdRegex))?.ToArray();
        if (changeIds == null || !changeIds.Any())
        {
            throw new ChangeIdNotFoundException(ErrorMessages.ChangeIdNotFound(null, false));
        }

        return changeIds;
    }

    private async Task UpdateChangesAsync(string organization, string projectId, string runId,
        IAzdoService azdoService, IEnumerable<string> changeIds)
    {
        var pipelineUrl = await azdoService.GetPipelineRunUrlAsync(organization, projectId, runId);

        if (string.IsNullOrEmpty(pipelineUrl))
        {
            throw new PipelineUrlNotFoundException("Pipeline url could not be retrieved from Azure DevOps.");
        }

        foreach (var changeId in changeIds)
        {
            await _changeClient.UpdateChangeAsync(new UpdateChangeRequestBody(changeId)
            {
                JournalUpdate = $"Azure DevOps release: {pipelineUrl}"
            });
        }
    }

    private async Task SetTagAsync(string organization, string projectId, string runId,
        string changeId, IAzdoService azdoService)
    {
        var body = new GetChangeByKeyRequestBody(changeId);
        var changeResponse = await _changeClient.GetChangeByKeyAsync(body);
        var url = changeResponse?.RetrieveChangeInfoByKey?.Information?[0].Url;
        if (string.IsNullOrEmpty(url))
        {
            throw new ChangeClientException(ErrorMessages.ChangeUrlNotReceived(changeResponse));
        }

        var urlHash = new Uri(url).ParseQueryString()["queryHash"];
        if (string.IsNullOrEmpty(urlHash))
        {
            throw new ChangeClientException(ErrorMessages.ChangeUrlNotReceived(changeResponse));
        }

        await azdoService.SetTagAsync(organization, projectId, runId, changeId, urlHash);
    }
}