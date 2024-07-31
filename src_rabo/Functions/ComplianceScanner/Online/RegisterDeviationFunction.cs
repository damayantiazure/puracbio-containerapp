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
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Helpers;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online;

public class RegisterDeviationFunction : BaseFunction
{
    private readonly IRegisterDeviationProcess _registerDeviationProcess;
    private readonly ILoggingService _loggingService;

    public RegisterDeviationFunction(IRegisterDeviationProcess registerDeviationProcess,
        ILoggingService loggingService, IHttpContextAccessor httpContextAccessor, ISecurityContext securityContext)
            : base(httpContextAccessor, loggingService, securityContext)
    {
        _registerDeviationProcess = registerDeviationProcess;
        _loggingService = loggingService;
    }

    [FunctionName(nameof(RegisterDeviationFunction))]
    public async Task<IActionResult> RegisterDeviation([HttpTrigger(AuthorizationLevel.Anonymous,
            WebRequestMethods.Http.Post,
            Route = "register-deviation/{organization}/{projectId}/{ciIdentifier}/{ruleName}/{itemId}/{foreignProjectId?}")]
        RegisterDeviationRequest registerDeviationRequest, HttpRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.Headers.Authorization.Count < 1)
            {
                return new UnauthorizedResult();
            }

            var authenticationHeader = AuthenticationHeaderValue.Parse(request.Headers.Authorization[0]);
            await _registerDeviationProcess.RegisterDeviation(registerDeviationRequest, authenticationHeader,
                cancellationToken);

            return new OkResult();
        }
        catch (Exception ex)
        {
            var exceptionBaseMetaInformation = request.ToExceptionBaseMetaInformation(
                registerDeviationRequest.Organization, registerDeviationRequest.ProjectId,
                nameof(RegisterDeviationFunction));
            await _loggingService.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog, exceptionBaseMetaInformation, registerDeviationRequest.ItemId,
                registerDeviationRequest.RuleName, ex);
            throw;
        }
    }
}