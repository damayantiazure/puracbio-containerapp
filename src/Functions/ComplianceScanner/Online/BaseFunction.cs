#nullable enable

#pragma warning disable CS0618 // The team assessed the risk of using these interfaces and decided 
// it's better than running isolated processes (which is the alternative and supports middleware).

// This has been in preview for years and Microsoft utters the intention of removing the obsolete
// attribute rather than removing the functionality. Please refer to the discussion on
// https://github.com/Azure/azure-webjobs-sdk/issues/2546 for more information.

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Primitives;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Requests.RequestValidation;
using Rabobank.Compliancy.Application.Security;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Helpers;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Validation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online;

public abstract class BaseFunction : IFunctionInvocationFilter, IFunctionExceptionFilter
{
    private const string _defaultContentType = "text/plain; charset=utf-8";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILoggingService _loggingService;
    private readonly ISecurityContext _securityContext;

    protected BaseFunction(IHttpContextAccessor httpContextAccessor, ILoggingService loggingService,
        ISecurityContext securityContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _loggingService = loggingService;
        _securityContext = securityContext;
    }

    public async Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
    {
        var requestBase = executingContext.Arguments.First().Value as RequestBase;

        if (requestBase?.Organization == null)
        {
            await SendBadRequestResponseAsync($"{nameof(requestBase.Organization)} parameter is not provided", cancellationToken);
            return;
        }

        await SetSecurityInformationAsync(requestBase!.Organization, cancellationToken);

        // First see if this call has an argument (it should be the first one) that is validatable.
        if (executingContext.Arguments.First().Value is not IValidatable validatableRequest)
        {
            return;
        }

        // Validate the request. If it's valid, just exit this method.
        var validationResult = validatableRequest.Validate();
        if (validationResult.IsValid)
        {
            return;
        }

        // If it's not valid, convert it into an exception.
        var exception = validationResult.ToException();

        // Then, check if the first argument of the function call is convertible to an ExceptionReport.
        if (executingContext.Arguments.First().Value is IExceptionReportConvertible convertibleRequest)
        {
            // If so, convert it and log the report.
            var exceptionReport = convertibleRequest.ToExceptionReport(
                executingContext.FunctionName,
                _httpContextAccessor.HttpContext?.Request.Path!,
                exception
            );
            await _loggingService.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog, exceptionReport);
        }

        // Eventually we throw the exception to stop the function from being executed. Execution continues in the OnExceptionAsync below.
        throw exception;
    }

    public async Task OnExceptionAsync(FunctionExceptionContext exceptionContext, CancellationToken cancellationToken)
    {
        if (exceptionContext.Exception.InnerException is ValidationErrorsException validationErrorsException)
        {
            SetResponseInformation(StatusCodes.Status400BadRequest, validationErrorsException.Message.Length);
            await ResolveResponseAsync(validationErrorsException.Message, cancellationToken);
        }
        else if (exceptionContext.Exception.InnerException is ItemAlreadyExistsException itemAlreadyExistsException)
        {
            SetResponseInformation(StatusCodes.Status409Conflict, itemAlreadyExistsException.Message.Length);
            await ResolveResponseAsync(itemAlreadyExistsException.Message, cancellationToken);
        }
    }

    public Task OnExecutedAsync(FunctionExecutedContext executedContext, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    private void SetResponseInformation(int statusCode, long? contentLength = null, string contentType = _defaultContentType)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            return;
        }

        _httpContextAccessor.HttpContext.Response.ContentType = contentType;
        _httpContextAccessor.HttpContext.Response.ContentLength = contentLength;
        _httpContextAccessor.HttpContext.Response.StatusCode = statusCode;
    }

    private async Task ResolveResponseAsync(string content, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext != null)
        {
            await _httpContextAccessor.HttpContext.Response.WriteAsync(content, cancellationToken);
            await _httpContextAccessor.HttpContext.Response.CompleteAsync();
        }
    }

    private async Task SendBadRequestResponseAsync(string message, CancellationToken cancellationToken)
    {
        SetResponseInformation(StatusCodes.Status400BadRequest, message.Length);
        await ResolveResponseAsync(message, cancellationToken);
    }

    private async Task SetSecurityInformationAsync(string organization, CancellationToken cancellationToken)
    {
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization;
        if (authHeader?.Count > 0)
        {
            await _securityContext.ResolveUserFromToken(authHeader.Value, organization, cancellationToken);
        }
    }
}
#pragma warning restore CS0618 // Type or member is obsolete
