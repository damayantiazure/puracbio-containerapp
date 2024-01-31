#nullable enable

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.Approvals.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.Sm9Changes.Application;
using Rabobank.Compliancy.Functions.Sm9Changes.Exceptions;
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

namespace Rabobank.Compliancy.Functions.Sm9Changes;

public class CreateChangeFunction
{
    private readonly IChangeClient _changeClient;
    private readonly ILoggingService _loggingService;
    private readonly ISm9ChangesService _sm9ChangesService;
    private readonly Dictionary<string, IAzdoService> _azdoService;
    private readonly Dictionary<string, IAzdoApproverService> _azdoApproverService;

    public CreateChangeFunction(
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

    [FunctionName(nameof(CreateChangeFunction))]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post",
            Route = "create-change/{organization}/{projectId}/{pipelineType}/{runId}")]
        HttpRequestMessage request, string? organization, string? projectId, string? pipelineType, string? runId)
    {
        try
        {
            _sm9ChangesService.ValidateFunctionInput(request, organization, projectId, pipelineType, runId);

            var input = await GetUserInputAsync(request);

            var azdoApproverService = _azdoApproverService[pipelineType];
            var (pipelineApprovers, pullRequestApprovers) =
                await azdoApproverService.GetApproversAsync(organization, projectId, runId);
            if (!pipelineApprovers.Any() && !pullRequestApprovers.Any())
            {
                throw new ApproverNotFoundException(ErrorMessages.ApproverNotFound(pipelineType, true));
            }

            var azdoService = _azdoService[pipelineType];
            var initiator = await azdoService.GetPipelineRunInitiatorAsync(organization, projectId, runId);
            if (string.IsNullOrEmpty(initiator))
            {
                throw new InitiatorNotFoundException(ErrorMessages.InitiatorNotFound());
            }

            var pipelineUrl = await azdoService.GetPipelineRunUrlAsync(organization, projectId, runId)
                              ?? throw new PipelineUrlNotFoundException(
                                  ErrorMessages.PipelineUrlNotFound(organization, projectId, runId));

            var changeId = await CreateChangeAsync(organization, input, initiator, pipelineUrl);

            await SetTagAsync(organization, projectId, runId, changeId, azdoService);

            await _sm9ChangesService.ApproveChangesAsync(
                organization, new[] { changeId }, pipelineApprovers, pullRequestApprovers);

            return CreateResponseHelper.CreateResponseTask(changeId, true);
        }
        catch (ChangeClientException ex)
        {
            var exceptionBaseMetaInformation = await LogExceptionAsync(ex);
            return CreateResponseHelper.CreateResponseTask(ErrorMessages.Sm9TeamError(
                $"{ex.Message} (CorrelationId:{exceptionBaseMetaInformation.CorrelationId})"), false);
        }
        catch (Exception ex) when (
            ex
                is ApproverNotFoundException
                or ArgumentException
                or InitiatorNotFoundException
                or PipelineUrlNotFoundException)
        {
            var exceptionBaseMetaInformation = await LogExceptionAsync(ex);
            return CreateResponseHelper.CreateResponseTask(
                $"{ex.Message} (CorrelationId:{exceptionBaseMetaInformation.CorrelationId})", false);
        }
        catch (Exception ex)
        {
            await LogExceptionAsync(ex);
            throw;
        }

        async Task<ExceptionBaseMetaInformation> LogExceptionAsync(Exception e)
        {
            var exceptionBaseMetaInformation = new ExceptionBaseMetaInformation
                (request, nameof(CreateChangeFunction), projectId)
                {
                    Organization = organization,
                    PipelineType = pipelineType,
                    RunId = runId
                };

            await _loggingService.LogExceptionAsync(LogDestinations.Sm9ChangesErrorLog, exceptionBaseMetaInformation,
                e);
            return exceptionBaseMetaInformation;
        }
    }

    private static async Task<CreateChangeDetails> GetUserInputAsync(HttpRequestMessage request)
    {
        if (request.Content == null)
        {
            throw new InvalidOperationException("Cannot retrieve content from request.");
        }

        var content = await request.Content.ReadAsStringAsync();
        var input = JsonConvert.DeserializeObject<CreateChangeDetails>(content);
        return input ?? throw new InvalidOperationException("Cannot retrieve content from request.");
    }

    private async Task<string> CreateChangeAsync(string organization,
        CreateChangeDetails input, string initiator, string pipelineUrl)
    {
        var createChange = new CreateChangeRequestBody(input.PriorityTemplate, input.Assets)
        {
            Description = input.Description,
            Title = input.Title,
            JournalUpdate = input.ImplementationPlan
                .Concat(new[] { Environment.NewLine + $"Azure DevOps release: {pipelineUrl}" })
                .ToArray(),
            Initiator = initiator,
            Requestor = initiator
        };

        var createChangeResponse = await CreateChangeAsync(organization, createChange);

        var changeId = createChangeResponse?.ChangeData?.ChangeId;
        if (string.IsNullOrEmpty(changeId))
        {
            throw new ChangeClientException(ErrorMessages.ChangeIdNotReceived(createChangeResponse));
        }

        return changeId;
    }

    private async Task<CreateChangeResponse?> CreateChangeAsync(
        string organization, CreateChangeRequestBody createChange)
    {
        try
        {
            return await _changeClient.CreateChangeAsync(createChange);
        }
        // Dirty fix for account issue North America, where most .com accounts don't have access to SM9
        catch (ChangeClientException e)
        {
            if (!IsNorthAmericaIssue(organization, e))
            {
                throw;
            }

            if (createChange.Initiator == null)
            {
                throw new ArgumentNullException(nameof(createChange.Initiator), e);
            }

            if (createChange.Requestor == null)
            {
                throw new ArgumentNullException(nameof(createChange.Requestor), e);
            }

            try
            {
                //Try creation with @raboag.com account
                createChange.Initiator = ReplaceWithOtherAccount(
                    createChange.Initiator, "@rabobank.com", "@raboag.com");
                createChange.Requestor = ReplaceWithOtherAccount(
                    createChange.Requestor, "@rabobank.com", "@raboag.com");
                return await _changeClient.CreateChangeAsync(createChange);
            }
            catch (ChangeClientException ex)
            {
                if (!IsNorthAmericaIssue(organization, ex))
                {
                    throw;
                }

                //Try creation with @rabo.com account
                createChange.Initiator = ReplaceWithOtherAccount(
                    createChange.Initiator, "@raboag.com", "@rabo.com");
                createChange.Requestor = ReplaceWithOtherAccount(
                    createChange.Requestor, "@raboag.com", "@rabo.com");
                return await _changeClient.CreateChangeAsync(createChange);
            }
        }
    }

    private static bool IsNorthAmericaIssue(string organization, Exception e) =>
        organization == "raboweb-na" &&
        e.Message.Contains("Please make sure that you have entered a valid operator name as requestor.");

    private static string ReplaceWithOtherAccount(
        string input, string oldEmailDomain, string newEmailDomain) =>
        input.Replace(oldEmailDomain, newEmailDomain,
            StringComparison.InvariantCultureIgnoreCase);

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