#nullable enable

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Security;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Helpers;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online;

public class ExclusionListFunction : BaseFunction
{
    private readonly IExclusionListProcess _exclusionListProcess;
    private readonly ILoggingService _loggingService;
    private readonly ICheckAuthorizationProcess _checkAuthorizationProcess;

    public ExclusionListFunction(
        IExclusionListProcess exclusionListProcess, ICheckAuthorizationProcess checkAuthorizationProcess,
        ILoggingService loggingService, IHttpContextAccessor httpContextAccessor,
        ISecurityContext securityContext) : base(httpContextAccessor, loggingService, securityContext)
    {
        _exclusionListProcess = exclusionListProcess;
        _checkAuthorizationProcess = checkAuthorizationProcess;
        _loggingService = loggingService;
    }

    /// <summary>
    /// CreateOrUpdateExclusionListAsync will create or update a exclusion record.
    /// </summary>
    /// <param name="exclusionListRequest">The request object is read from the body of the DELETE request and converted to <see cref="ExclusionListRequest"/>.</param>
    /// <param name="httpRequest">Represents the incoming side of an individual HTTP request.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The result of the action as <see cref="IActionResult"/>.</returns>
    [FunctionName(nameof(ExclusionListFunction))]
    public Task<IActionResult> CreateOrUpdateExclusionListAsync([HttpTrigger(AuthorizationLevel.Anonymous,
        Route = "exclusion-list/{organization}/{projectId}/{pipelineId}/{pipelineType}")]
         ExclusionListRequest exclusionListRequest, HttpRequest httpRequest, CancellationToken cancellationToken = default) =>
            PerformCreateOrUpdateExclusionAsync(exclusionListRequest, httpRequest, nameof(ExclusionListFunction), cancellationToken);

    /// <summary>
    /// PostCreateOrUpdateExclusionListAsync will create or update a exclusion record.
    /// </summary>
    /// <param name="exclusionListRequest">The request object is read from the body of the DELETE request and converted to <see cref="ExclusionListRequest"/>.</param>
    /// <param name="httpRequest">Represents the incoming side of an individual HTTP request.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The result of the action as <see cref="IActionResult"/>.</returns>
    [FunctionName(nameof(PostCreateOrUpdateExclusionListAsync))]
    public Task<IActionResult> PostCreateOrUpdateExclusionListAsync([HttpTrigger(AuthorizationLevel.Anonymous, WebRequestMethods.Http.Post)]
        ExclusionListRequest exclusionListRequest, HttpRequest httpRequest, CancellationToken cancellationToken = default) =>
            PerformCreateOrUpdateExclusionAsync(exclusionListRequest, httpRequest, nameof(PostCreateOrUpdateExclusionListAsync), cancellationToken);

    private async Task<IActionResult> PerformCreateOrUpdateExclusionAsync(ExclusionListRequest exclusionListRequest, HttpRequest httpRequest,
       string functionName, CancellationToken cancellationToken = default)
    {
        try
        {
            var authorizationHeader = httpRequest.GetAuthorizationTokenOrDefault();
            var authorizationRequest =
                new AuthorizationRequest(exclusionListRequest.ProjectId, exclusionListRequest.Organization);
            var userPermission =
                await _checkAuthorizationProcess.GetUserPermissionAsync(authorizationRequest, authorizationHeader,
                    cancellationToken);

            // check if user has edit permissions
            if (userPermission == null || !userPermission.IsAllowedToEditPermissions)
            {
                return new UnauthorizedResult();
            }

            var resultMessage =
                await _exclusionListProcess.CreateOrUpdateExclusionListAsync(exclusionListRequest, userPermission.User,
                    cancellationToken);

            return new OkObjectResult(resultMessage);
        }
        catch (Exception ex) when (ex is InvalidExclusionRequesterException or ExclusionApproverAlreadyExistsException)
        {
            var exceptionBaseMetaInformation = await LogExceptionAsync(ex);
            return new BadRequestObjectResult(
                $"{ex.Message} (CorrelationId:{exceptionBaseMetaInformation.CorrelationId})");
        }
        catch (Exception ex)
        {
            await LogExceptionAsync(ex);
            throw;
        }

        async Task<ExceptionBaseMetaInformation> LogExceptionAsync(Exception ex)
        {
            var exceptionBaseMetaInformation = httpRequest.ToExceptionBaseMetaInformation(
                exclusionListRequest.Organization, exclusionListRequest.ProjectId, functionName);

            await _loggingService.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog, ex,
                exceptionBaseMetaInformation, exclusionListRequest.PipelineId.ToString(), exclusionListRequest.PipelineType);
            return exceptionBaseMetaInformation;
        }
    }
}