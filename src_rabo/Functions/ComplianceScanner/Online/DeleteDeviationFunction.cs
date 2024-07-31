#nullable enable

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Interfaces.Deviations;
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

public class DeleteDeviationFunction : BaseFunction
{
    private readonly ICheckAuthorizationProcess _checkAuthorizationProcess;
    private readonly IDeleteDeviationProcess _deleteDeviationProcess;
    private readonly ILoggingService _loggingService;

    public DeleteDeviationFunction(ICheckAuthorizationProcess checkAuthorizationProcess,
        IDeleteDeviationProcess deleteDeviationProcess, IHttpContextAccessor httpContextAccessor,
        ILoggingService loggingService, ISecurityContext securityContext)
        : base(httpContextAccessor, loggingService, securityContext)
    {
        _checkAuthorizationProcess = checkAuthorizationProcess;
        _deleteDeviationProcess = deleteDeviationProcess;
        _loggingService = loggingService;
    }

    /// <summary>
    /// RunAsync will delete a deviation that has been registered on a specific rule.
    /// </summary>
    /// <param name="deleteDeviationRequest">The request object is read from the body of the DELETE request and converted to <see cref="DeleteDeviationRequest"/>.</param>
    /// <param name="httpRequest">Represents the incoming side of an individual HTTP request.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The result of the action as <see cref="IActionResult"/>.</returns>
    [FunctionName(nameof(DeleteDeviationFunction))]
    public Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete",
            Route = "delete-deviation/{organization}/{projectId}/{ciIdentifier}/{ruleName}/{itemId}/{foreignProjectId?}")]
       DeleteDeviationRequest deleteDeviationRequest, HttpRequest httpRequest, CancellationToken cancellationToken = default) =>
        PerformProcessHandlerAsync(deleteDeviationRequest, httpRequest, nameof(DeleteDeviationFunction), cancellationToken);

    /// <summary>
    /// DeleteDeviationAsync will delete a deviation that has been registered on a specific rule.
    /// </summary>
    /// <param name="deleteDeviationRequest">The request object is read from the body of the DELETE request and converted to <see cref="DeleteDeviationRequest"/>.</param>
    /// <param name="httpRequest">Represents the incoming side of an individual HTTP request.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The result of the action as <see cref="IActionResult"/>.</returns>
    [FunctionName(nameof(DeleteDeviationAsync))]
    public Task<IActionResult> DeleteDeviationAsync(
       [HttpTrigger(AuthorizationLevel.Anonymous, "delete")]
        DeleteDeviationRequest deleteDeviationRequest, HttpRequest httpRequest, CancellationToken cancellationToken = default) =>
        PerformProcessHandlerAsync(deleteDeviationRequest, httpRequest, nameof(DeleteDeviationAsync), cancellationToken);

    private async Task<IActionResult> PerformProcessHandlerAsync(DeleteDeviationRequest deleteDeviationRequest, HttpRequest httpRequest,
        string functionName, CancellationToken cancellationToken = default)
    {
        try
        {
            var authorizationHeader = httpRequest.GetAuthorizationTokenOrDefault();
            var authorizationRequest = new AuthorizationRequest(deleteDeviationRequest.ProjectId, deleteDeviationRequest.Organization);

            if (!await _checkAuthorizationProcess.IsAuthorized(authorizationRequest, authorizationHeader, cancellationToken))
            {
                return new UnauthorizedResult();
            }

            await _deleteDeviationProcess.DeleteDeviationAsync(deleteDeviationRequest, cancellationToken);

            return new OkResult();
        }
        // Ignore cases where the deviation is not present anymore
        catch (StorageException storageException) when (storageException?.RequestInformation?.HttpStatusCode == (int)HttpStatusCode.NotFound)
        {
            return new OkResult();
        }
        catch (ArgumentException ex)
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
            var exceptionBaseMetaInformation = httpRequest.ToExceptionBaseMetaInformation(deleteDeviationRequest.Organization,
                deleteDeviationRequest.ProjectId, functionName);
            await _loggingService.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog, exceptionBaseMetaInformation, ex, deleteDeviationRequest.ItemId, deleteDeviationRequest.RuleName, deleteDeviationRequest.CiIdentifier);
            return exceptionBaseMetaInformation;
        }
    }
}