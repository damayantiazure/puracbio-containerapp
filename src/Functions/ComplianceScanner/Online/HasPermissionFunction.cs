#nullable enable

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ErrorMessages = Rabobank.Compliancy.Functions.ComplianceScanner.Online.Exceptions.ErrorMessages;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online;

public class HasPermissionFunction
{
    private const string _runAsyncRoute = "has-permissions/{organization}/{projectId}";
    private const string _validationErrorsOccurred = "Errors validating the request.";
    private readonly ICheckAuthorizationProcess _checkAuthorizationProcess;
    private readonly ILoggingService _loggingService;

    public HasPermissionFunction(
        ICheckAuthorizationProcess checkAuthorizationProcess,
        ILoggingService loggingService)
    {
        _checkAuthorizationProcess = checkAuthorizationProcess;
        _loggingService = loggingService;
    }

    [FunctionName(nameof(HasPermissionFunction))]
    public async Task<IActionResult> HasPermission([HttpTrigger(AuthorizationLevel.Anonymous,
            Route = _runAsyncRoute)]
        HttpRequestMessage request, string organization, Guid projectId, CancellationToken cancellationToken)
    {
        try
        {
            ValidateRequest(request, organization, projectId);

            var authorizationRequest = new AuthorizationRequest(projectId, organization);

            var result = await _checkAuthorizationProcess.IsAuthorized(authorizationRequest,
                request.Headers.Authorization, cancellationToken);

            return new OkObjectResult(result);
        }
        catch (TokenInvalidException ex)
        {
            var exceptionBaseMetaInformation = await LogExceptionAsync(ex);
            return new UnauthorizedObjectResult(
                $"{ErrorMessages.SessionInvalid(ex.Message)} (CorrelationId:{exceptionBaseMetaInformation.CorrelationId})");
        }
        catch (AggregateException ex) when (ex.InnerExceptions.Any(exception => exception is ArgumentException))
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
            var exceptionBaseMetaInformation =
                request.ToExceptionBaseMetaInformation(organization, projectId, nameof(HasPermissionFunction));
            await _loggingService.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog, exceptionBaseMetaInformation, ex);
            return exceptionBaseMetaInformation;
        }
    }

    private static void ValidateRequest(HttpRequestMessage request, string organization, Guid projectId)
    {
        var exceptions = new List<Exception>();
        if (string.IsNullOrEmpty(organization))
        {
            exceptions.Add(new ArgumentNullException(nameof(organization)));
        }

        if (projectId == Guid.Empty)
        {
            exceptions.Add(new ArgumentNullException(nameof(projectId)));
        }

        if (request.Headers.Authorization?.Parameter == null)
        {
            exceptions.Add(new ArgumentNullException(nameof(request)));
        }

        if (exceptions.Any())
        {
            throw new AggregateException(_validationErrorsOccurred, exceptions);
        }
    }
}