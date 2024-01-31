#nullable enable

using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.StorageClient;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using PipelineProcessType = Rabobank.Compliancy.Infra.AzdoClient.Model.Constants.PipelineProcessType;
using Response = Rabobank.Compliancy.Infra.AzdoClient.Response;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;

public class ManageHooksService : IManageHooksService
{
    private const string _sensesFailingPullRequestHookId = "a802e974-9d78-476f-bd15-89ebe973ee0d";
    private const string _pullRequestMergedEventType = "git.pullrequest.merged";
    private const string _classicReleaseEventType = "ms.vss-release.deployment-completed-event";
    private const string _yamlReleaseEventType = "ms.vss-pipelines.stage-state-changed-event";
    private const int _parallelProjects = 10;
    private const int _parallelHooks = 20;
    private const string _hookFailureMessage = "Not delivered because event didn't match the specified filter or the subscription owner doesn't have sufficient permissions.";

    private readonly IAzdoRestClient _azdoClient;
    private readonly StorageClientConfig _storageClientConfig;
    private readonly ILoggingService _loggingService;
    private readonly IPipelineRegistrationRepository _registrationRepository;

    public ManageHooksService(
        IAzdoRestClient azdoClient,
        StorageClientConfig storageClientConfig,
        ILoggingService loggingService,
        IPipelineRegistrationRepository registrationRepository)
    {
        _azdoClient = azdoClient;
        _storageClientConfig = storageClientConfig;
        _loggingService = loggingService;
        _registrationRepository = registrationRepository;
    }

    public async Task ManageHooksOrganizationAsync(string organization)
    {
        try
        {
            var hooks = await GetOrganizationHooksAsync(organization);
            await LogHookFailuresAsync(organization, hooks
                .Where(hook => hook.Id != _sensesFailingPullRequestHookId));

            var projects = await _azdoClient.GetAsync(Project.Projects(), organization);
            var semaphoreSlim = new SemaphoreSlim(_parallelProjects);
            await Task.WhenAll(projects.Select(async project =>
            {
                await semaphoreSlim.WaitAsync();
                try
                {
                    var projectHooks = hooks
                        .Where(hook => hook.PublisherInputs.ProjectId == project.Id);
                    await ManageHooksProjectAsync(organization, project.Id, projectHooks);
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }));
        }
        catch (Exception ex)
        {
            var exceptionBaseMetaInformation = new ExceptionBaseMetaInformation(nameof(ManageHooksService), organization);
            await _loggingService.LogExceptionAsync(LogDestinations.AuditLoggingErrorLog, exceptionBaseMetaInformation, ex);
            throw;
        }
    }

    public async Task CreateHookAsync(string organization, string projectId, string pipelineType, string pipelineId)
    {
        try
        {
            var hooks = await GetOrganizationHooksAsync(organization);

            var projectHooks = hooks
                .Where(hook => hook.PublisherInputs.ProjectId == projectId);

            if (pipelineType == ItemTypes.ClassicReleasePipeline)
            {
                await CreateClassicHooksAsync(organization, projectId, projectHooks, new[] { pipelineId });
            }
            if (pipelineType == ItemTypes.YamlReleasePipeline)
            {
                await CreateYamlHooksAsync(organization, projectId, projectHooks, new[] { pipelineId });
            }
        }
        catch (Exception ex)
        {
            var exceptionBaseMetaInformation = new ExceptionBaseMetaInformation(nameof(ManageHooksService), organization, projectId);
            await _loggingService.LogExceptionAsync(LogDestinations.AuditLoggingHookFailureLog, ex, exceptionBaseMetaInformation,
                pipelineId, pipelineType);
            throw;
        }
    }

    private async Task<IEnumerable<Response.Hook>> GetOrganizationHooksAsync(string organization)
    {
        var hooks = await _azdoClient.GetAsync(Hooks.Subscriptions(), organization);
        return hooks
            .Where(hook => hook.ConsumerInputs.AccountName ==
                        _storageClientConfig.EventQueueStorageAccountName);
    }

    private async Task ManageHooksProjectAsync(
        string organization, string projectId, IEnumerable<Response.Hook> hooks)
    {
        await CreatePullRequestUpdatedHookAsync(organization, projectId, hooks);

        var pipelineRegistrations = await _registrationRepository.GetAsync(
            organization, projectId);

        if (!hooks.Any() && !pipelineRegistrations.Any())
        {
            return;
        }

        var registeredClassicPipelineIds = await GetRegisteredClassicPipelineIdsAsync(
            organization, projectId, pipelineRegistrations);
        var registeredYamlPipelineIds = await GetRegisteredYamlPipelineIdsAsync(
            organization, projectId, pipelineRegistrations);

        await DeleteHooksAsync(organization, hooks,
            registeredClassicPipelineIds, registeredYamlPipelineIds);

        await CreateClassicHooksAsync(organization, projectId, hooks, registeredClassicPipelineIds);
        await CreateYamlHooksAsync(organization, projectId, hooks, registeredYamlPipelineIds);
    }

    private async Task CreatePullRequestUpdatedHookAsync(string organization, string projectId,
        IEnumerable<Response.Hook> hooks)
    {
        if (!PullRequestUpdatedHookExists(projectId, hooks))
        {
            var body = Hooks.Add.GitPullRequestMerged(
                _storageClientConfig.EventQueueStorageAccountName,
                _storageClientConfig.EventQueueStorageAccountKey,
                StorageQueueNames.AuditPullRequestApproversQueueName,
                projectId);

            await _azdoClient.PostAsync(Hooks.AddHookSubscription(), body, organization);
        }
    }

    private static bool PullRequestUpdatedHookExists(string projectId, IEnumerable<Response.Hook> auditLoggingHooks) =>
        auditLoggingHooks
            .Where(h => h.EventType == _pullRequestMergedEventType)
            .Select(h => h.PublisherInputs.ProjectId)
            .Contains(projectId);

    private async Task<IEnumerable<string>> GetRegisteredClassicPipelineIdsAsync(
        string organization, string projectId, IEnumerable<PipelineRegistration> pipelineRegistrations)
    {
        var allClassicPipelineIds = (await _azdoClient.GetAsync(ReleaseManagement.Definitions(
                projectId), organization))
            .Select(releaseDefinition => releaseDefinition.Id);
        return pipelineRegistrations
            .Where(pipelineRegistration => pipelineRegistration.StageId != null &&
                        pipelineRegistration.IsClassicReleasePipeline() &&
                        PipelineExists(pipelineRegistration, allClassicPipelineIds))
            .Select(pipelineRegistration => pipelineRegistration.PipelineId)
            .Distinct();
    }

    private async Task<IEnumerable<string>> GetRegisteredYamlPipelineIdsAsync(
        string organization, string projectId, IEnumerable<PipelineRegistration> pipelineRegistrations)
    {
        var allYamlPipelineIds = (await _azdoClient.GetAsync(Builds.BuildDefinitions(
                projectId, PipelineProcessType.YamlPipeline), organization))
            .Select(buildDefinition => buildDefinition.Id);
        return pipelineRegistrations
            .Where(pipelineRegistration => pipelineRegistration.StageId != null &&
                        !pipelineRegistration.IsClassicReleasePipeline() &&
                        PipelineExists(pipelineRegistration, allYamlPipelineIds))
            .Select(pipelineRegistration => pipelineRegistration.PipelineId)
            .Distinct();
    }

    private static bool PipelineExists(PipelineRegistration registration, IEnumerable<string> pipelines) =>
        pipelines
            .Any(pipelineIs => pipelineIs == registration.PipelineId);

    private async Task DeleteHooksAsync(string organization, IEnumerable<Response.Hook> hooks,
        IEnumerable<string> registeredClassicPipelineIds, IEnumerable<string> registeredYamlPipelineIds)
    {
        var hooksToRemove = GetDuplicateHooks(hooks)
            .Concat(GetInvalidClassicHooks(registeredClassicPipelineIds, hooks))
            .Concat(GetInvalidYamlHooks(registeredYamlPipelineIds, hooks))
            .Distinct();

        await Task.WhenAll(hooksToRemove.Select(async hook =>
            await _azdoClient.DeleteAsync(Hooks.Subscription(hook.Id), organization)));
    }

    private static IEnumerable<Response.Hook> GetDuplicateHooks(IEnumerable<Response.Hook> hooks) =>
        hooks
            .Except(hooks
                .GroupBy(hook => new
                {
                    hook.PublisherInputs.ReleaseDefinitionId,
                    hook.PublisherInputs.PipelineId
                })
                .Select(grouping => grouping.First()));

    private static IEnumerable<Response.Hook> GetInvalidClassicHooks(
        IEnumerable<string> registeredPipelineIds, IEnumerable<Response.Hook> hooks) =>
        hooks
            .Where(hook => hook.EventType == _classicReleaseEventType &&
                        !registeredPipelineIds.Contains(hook.PublisherInputs.ReleaseDefinitionId));

    private static IEnumerable<Response.Hook> GetInvalidYamlHooks(
        IEnumerable<string> registeredPipelineIds, IEnumerable<Response.Hook> hooks) =>
        hooks
            .Where(hook => hook.EventType == _yamlReleaseEventType &&
                        !registeredPipelineIds.Contains(hook.PublisherInputs.PipelineId));

    private async Task CreateClassicHooksAsync(string organization, string projectId,
        IEnumerable<Response.Hook> hooks, IEnumerable<string> registeredPipelineIds)
    {
        var classicPipelinesWithoutHooks = registeredPipelineIds
            .Where(pipelineId => !ClassicReleaseHookExists(pipelineId, hooks));

        var semaphoreSlim = new SemaphoreSlim(_parallelHooks);
        await Task.WhenAll(classicPipelinesWithoutHooks.Select(async pipelineId =>
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                await CreateClassicReleaseHookAsync(organization, projectId, pipelineId);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }));
    }

    private static bool ClassicReleaseHookExists(string pipelineId, IEnumerable<Response.Hook> auditLoggingHooks) =>
        auditLoggingHooks
            .Where(hook => hook.EventType == _classicReleaseEventType)
            .Select(hook => hook.PublisherInputs.ReleaseDefinitionId)
            .Contains(pipelineId);

    private async Task CreateClassicReleaseHookAsync(string organization, string projectId, string pipelineId)
    {
        var body = Hooks.Add.ReleaseDeploymentCompleted(
            _storageClientConfig.EventQueueStorageAccountName,
            _storageClientConfig.EventQueueStorageAccountKey,
            StorageQueueNames.AuditClassicReleaseQueueName,
            projectId,
            pipelineId);

        await _azdoClient.PostAsync(Hooks.AddReleaseManagementSubscription(), body, organization);
    }

    private async Task CreateYamlHooksAsync(string organization, string projectId,
        IEnumerable<Response.Hook> hooks, IEnumerable<string> registeredPipelineIds)
    {
        var yamlPipelinesWithoutHooks = registeredPipelineIds
            .Where(pipelineId => !YamlReleaseHookExists(pipelineId, hooks));

        var semaphoreSlim = new SemaphoreSlim(_parallelHooks);
        await Task.WhenAll(yamlPipelinesWithoutHooks.Select(async pipelineId =>
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                await CreateYamlReleaseHookAsync(organization, projectId, pipelineId);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }));
    }

    private static bool YamlReleaseHookExists(string pipelineId, IEnumerable<Response.Hook> auditLoggingHooks) =>
        auditLoggingHooks
            .Where(hook => hook.EventType == _yamlReleaseEventType)
            .Select(hook => hook.PublisherInputs.PipelineId)
            .Contains(pipelineId);

    private async Task CreateYamlReleaseHookAsync(string organization, string projectId, string pipelineId)
    {
        var body = Hooks.Add.RunStageCompleted(
            _storageClientConfig.EventQueueStorageAccountName,
            _storageClientConfig.EventQueueStorageAccountKey,
            StorageQueueNames.AuditYamlReleaseQueueName,
            projectId,
            pipelineId);

        await _azdoClient.PostAsync(Hooks.AddHookSubscription(), body, organization);
    }

    private async Task LogHookFailuresAsync(string organization, IEnumerable<Response.Hook> hooks)
    {
        var semaphoreSlim = new SemaphoreSlim(_parallelHooks);
        await Task.WhenAll(hooks.Select(async hook =>
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                var hookFailures = (await _azdoClient.GetAsync(Hooks.HookNotifications(hook.Id), organization))
                    .Where(notification => notification.CreatedDate.Date == DateTime.Now.Date.AddDays(-1) &&
                                (notification.Result == Response.NotificationResult.failed ||
                                 notification.Result == Response.NotificationResult.filtered));

                await Task.WhenAll(hookFailures
                    .Select(async n => await LogHookFailureAsync(organization, n)));
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }));
    }

    private async Task LogHookFailureAsync(string organization, Response.Notification hookFailure)
    {
        var hookFailureWithDetails = await _azdoClient.GetAsync(Hooks.HookNotification(
            hookFailure.SubscriptionId, hookFailure.Id), organization);

        var hookFailureReport = new HookFailureReport
        {
            Date = hookFailureWithDetails.CreatedDate,
            ErrorMessage = hookFailureWithDetails.Details?.ErrorMessage ?? _hookFailureMessage,
            ErrorDetail = hookFailureWithDetails.Details?.ErrorDetail,
            Organization = organization,
            ProjectId = hookFailureWithDetails.Details?.PublisherInputs?.ProjectId == null
                ? null
                : Guid.Parse(hookFailureWithDetails.Details.PublisherInputs.ProjectId),
            PipelineId = hookFailureWithDetails.Details?.PublisherInputs?.PipelineId ??
                         hookFailureWithDetails.Details?.PublisherInputs?.ReleaseDefinitionId,
            HookId = hookFailureWithDetails.SubscriptionId,
            EventId = hookFailureWithDetails.Details?.Event.Id,
            EventType = hookFailureWithDetails.Details?.EventType,
            EventResourceData = hookFailureWithDetails.Details?.Event?.Resource?.ToString()
        };

        await _loggingService.LogInformationAsync(LogDestinations.AuditLoggingHookFailureLog, hookFailureReport);
    }
}