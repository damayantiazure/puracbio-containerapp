#nullable enable

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;
using Rabobank.Compliancy.Infra.StorageClient;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;

public class ExclusionStorageRepository : IExclusionStorageRepository
{
    private const string _partitionKey = "Exclusion";
    private const string _table = "ExclusionList";
    private readonly IAuthorizationService _authorizationService;
    private readonly Lazy<IStorageRepository> _lazyStorageRepository;

    public ExclusionStorageRepository(
        IStorageRepository storageRepository,
        IAuthorizationService authorizationService)
    {
        if (storageRepository == null)
        {
            throw new ArgumentNullException(nameof(storageRepository));
        }

        _lazyStorageRepository = new Lazy<IStorageRepository>(() =>
        {
            storageRepository.CreateTable(_table);
            return storageRepository;
        });

        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public async Task<Exclusion?> GetExclusionAsync(PipelineRunInfo runInfo)
    {
        var rowKey = Exclusion.CreateRowKey(runInfo);
        var result = await _lazyStorageRepository.Value.GetEntityAsync<Exclusion>(_partitionKey, rowKey);
        return result?.Result as Exclusion;
    }

    public Task SetRunIdAsync(PipelineRunInfo? runInfo)
    {
        if (runInfo == null)
        {
            throw new ArgumentNullException(nameof(runInfo));
        }

        return SetRunIdInternalAsync(runInfo);
    }

    public async Task<IActionResult> CreateExclusionAsync(HttpRequestMessage request, PipelineRunInfo runInfo)
    {
        var user = await _authorizationService.GetInteractiveUserAsync(request, runInfo.Organization);
        if (user == null)
        {
            throw new ArgumentException("User could not be found.");
        }

        var exclusionRequest = await request.Content.ReadAsAsync<ExclusionReport>();
        if (string.IsNullOrEmpty(exclusionRequest.Reason))
        {
            return new BadRequestObjectResult("No valid reason provided.");
        }

        var exclusion = new Exclusion(runInfo)
        {
            Organization = runInfo.Organization,
            ProjectId = runInfo.ProjectId,
            PipelineId = runInfo.PipelineId,
            PipelineType = runInfo.PipelineType,
            ExclusionReasonRequester = exclusionRequest.Reason,
            Requester = user.MailAddress,
            Approver = null,
            ExclusionReasonApprover = null,
            RunId = null
        };

        await _lazyStorageRepository.Value.InsertOrReplaceAsync(new List<Exclusion> { exclusion });
        return new OkObjectResult(
            "Exclusion has been registered. Please make sure you have someone else register an exclusion as well in order to finalize the exclusion!");
    }

    public async Task<IActionResult> UpdateExclusionAsync(HttpRequestMessage request, PipelineRunInfo runInfo)
    {
        var user = await _authorizationService.GetInteractiveUserAsync(request, runInfo.Organization) ??
                   throw new ArgumentException("User could not be found.");
        
        var exclusionRequest = await request.Content.ReadAsAsync<ExclusionReport>();
        if (string.IsNullOrEmpty(exclusionRequest.Reason))
        {
            return new BadRequestObjectResult("No valid reason provided.");
        }

        var exclusion = await GetExclusionAsync(runInfo);
        if (exclusion == null)
        {
            return new BadRequestObjectResult("Exclusion could not be found.");
        }

        if (user.MailAddress == exclusion.Requester)
        {
            return new BadRequestObjectResult(ErrorMessages.InvalidApprover);
        }

        if (exclusion.Approver != null)
        {
            return new BadRequestObjectResult(ErrorMessages.AlreadyApproved);
        }

        exclusion.Approver = user.MailAddress;
        exclusion.ExclusionReasonApprover = exclusionRequest.Reason;

        await UpdateExclusionAsync(exclusion);

        return new OkObjectResult("Exclusion request succesfully approved.");
    }

    private async Task SetRunIdInternalAsync(PipelineRunInfo runInfo)
    {
        var rowKey = Exclusion.CreateRowKey(runInfo);
        var result = await _lazyStorageRepository.Value.GetEntityAsync<Exclusion>(_partitionKey, rowKey);
        if (result?.Result is Exclusion exclusion)
        {
            exclusion.RunId = runInfo.RunId;
            await UpdateExclusionAsync(exclusion);
        }
    }

    private Task UpdateExclusionAsync(ITableEntity exclusion) =>
        _lazyStorageRepository.Value.InsertOrMergeAsync(exclusion);
}