#nullable enable

using Microsoft.Azure.WebJobs;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.StorageClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Response = Rabobank.Compliancy.Infra.AzdoClient.Response;

namespace Rabobank.Compliancy.Functions.AuditLogging;

public class DeleteHooksFunction
{
    private const int _parallelHooks = 20;
    private readonly string[] _organizations;
    private readonly IAzdoRestClient _azdoClient;
    private readonly ILoggingService _loggingService;
    private readonly StorageClientConfig _storageClientConfig;

    public DeleteHooksFunction(
        string[] organizations,
        IAzdoRestClient azdoClient,
        ILoggingService validateInputService,
        StorageClientConfig storageClientConfig)
    {
        _organizations = organizations;
        _azdoClient = azdoClient;
        _loggingService = validateInputService;
        _storageClientConfig = storageClientConfig;
    }

    [FunctionName(nameof(DeleteHooksFunction))]
    [NoAutomaticTrigger]
    public async Task RunAsync(string input)
    {
        await Task.WhenAll(_organizations.Select(async o =>
            await DeleteHooksOrganizationAsync(o)));
    }

    private async Task DeleteHooksOrganizationAsync(string organization)
    {
        try
        {
            var hooks = await GetOrganizationHooksAsync(organization);

            var semaphoreSlim = new SemaphoreSlim(_parallelHooks);
            await Task.WhenAll(hooks.Select(async (h, _) =>
            {
                await semaphoreSlim.WaitAsync();
                try
                {
                    await _azdoClient.DeleteAsync(Hooks.Subscription(h.Id), organization);
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }));
        }
        catch (Exception e)
        {
            var exceptionBaseMetaInformation = new ExceptionBaseMetaInformation(nameof(DeleteHooksFunction), organization);
            await _loggingService.LogExceptionAsync(LogDestinations.AuditLoggingErrorLog, exceptionBaseMetaInformation, e);
            throw;
        }
    }

    private async Task<IEnumerable<Response.Hook>> GetOrganizationHooksAsync(string organization)
    {
        var hooks = await _azdoClient.GetAsync(Hooks.Subscriptions(), organization);
        return hooks
            .Where(h => h.ConsumerInputs.AccountName ==
                        _storageClientConfig.EventQueueStorageAccountName);
    }
}