#nullable enable

using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Rabobank.Compliancy.Domain.Exceptions;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Helpers;

public static class ExceptionExtensions
{
    public static ExceptionBaseMetaInformation ToExceptionBaseMetaInformation(this HttpRequestMessage request,
        string? organization, Guid projectId, string functionName) =>
        new(functionName, organization, projectId.ToString(), request.RequestUri?.AbsoluteUri);

    public static ExceptionBaseMetaInformation ToExceptionBaseMetaInformation(this HttpRequest httpRequest,
        string? organization, Guid projectId, string functionName) =>
        new(functionName, organization, projectId.ToString(), httpRequest.Path);
}