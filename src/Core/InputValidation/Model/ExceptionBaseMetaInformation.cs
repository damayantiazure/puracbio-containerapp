#nullable enable

using System;
using System.Net.Http;
using Rabobank.Compliancy.Domain.Extensions;

namespace Rabobank.Compliancy.Core.InputValidation.Model;

public class ExceptionBaseMetaInformation
{
    public ExceptionBaseMetaInformation(string function, string? organization)
    {
        Function = function;
        Organization = organization;
    }

    public ExceptionBaseMetaInformation(string function, string? organization, string? projectId) :
        this(function, organization) =>
        ProjectId = projectId;

    public ExceptionBaseMetaInformation(string function, string? organization, string? projectId, string? requestUrl) :
        this(function, organization, projectId) =>
        RequestUrl = requestUrl;

    public ExceptionBaseMetaInformation(HttpRequestMessage requestMessage, string function, string? projectId)
    {
        Function = function;
        Request = requestMessage.StripSensitiveInformationFromHeader()?.ToString();
        RequestUrl = requestMessage.RequestUri?.AbsoluteUri;
        ProjectId = projectId;
    }

    public string Function { get; }
    public string? RequestUrl { get; }
    public string? Request { get; }
    public string? Organization { get; init; }
    public string? ProjectId { get; }
    public string? RunId { get; init; }
    public string? ReleaseId { get; init; }
    public string? PullRequestUrl { get; init; }
    public string? RunUrl { get; init; }
    public object? RequestData { get; init; }
    public string? PipelineType { get; init; }
    public string? ReleaseUrl { get; init; }
    public string CorrelationId { get; } = Guid.NewGuid().ToString();
}