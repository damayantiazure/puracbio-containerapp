#nullable enable

using System;
using System.Net.Http;
using Flurl.Http;

namespace Rabobank.Compliancy.Core.InputValidation.Model;

public class ExceptionReport
{
    public ExceptionReport(ExceptionBaseMetaInformation exceptionBaseMetaInformation, Exception exception)
        : this(exceptionBaseMetaInformation)
    {
        SetException(exception);
    }

    public ExceptionReport(ExceptionBaseMetaInformation exceptionBaseMetaInformation)
    {
        FunctionName = exceptionBaseMetaInformation.Function;
        RequestUrl = exceptionBaseMetaInformation.RequestUrl;
        Organization = exceptionBaseMetaInformation.Organization;
        ProjectId = exceptionBaseMetaInformation.ProjectId;
        RunId = exceptionBaseMetaInformation.RunId;
        ReleaseId = exceptionBaseMetaInformation.ReleaseId;
        ReleaseUrl = exceptionBaseMetaInformation.ReleaseUrl;
        RequestData = exceptionBaseMetaInformation.RequestData;
        PipelineType = exceptionBaseMetaInformation.PipelineType;
        PullRequestUrl = exceptionBaseMetaInformation.PullRequestUrl;
        Request = exceptionBaseMetaInformation.Request;
        RunUrl = exceptionBaseMetaInformation.RunUrl;
        CorrelationId = exceptionBaseMetaInformation.CorrelationId;
    }

    public ExceptionReport(Exception? exception)
    {
        CorrelationId = Guid.NewGuid().ToString();
        SetException(exception);
    }

    public ExceptionReport() => CorrelationId = Guid.NewGuid().ToString();

    public DateTime Date { get; } = DateTime.UtcNow;
    public string? FunctionName { get; init; }

    /// <summary>
    ///     Getter and setter that contains the <see cref="HttpRequestMessage" /> ToString().
    /// </summary>
    public string? Request { get; }

    public string? RequestUrl { get; init; }
    public object? RequestData { get; }
    public string? ExceptionType { get; set; }
    public string? ExceptionMessage { get; set; }
    public string? InnerExceptionType { get; set; }
    public string? InnerExceptionMessage { get; set; }
    public string? Organization { get; init; }
    public string? ProjectId { get; init; }

    /// <summary>
    ///     Getter and setter for the item identifier (PipelineId or RepositoryId)
    /// </summary>
    public string? ItemId { get; init; }

    /// <summary>
    ///     Getter and setter for the item type (PipelineType or RepositoryType)
    /// </summary>
    public string? ItemType { get; init; }

    public string? RuleName { get; init; }
    public string? CiIdentifier { get; init; }
    public Guid? UserId { get; init; }
    public string? UserMail { get; init; }
    public string? RunId { get; init; }
    public string? RunUrl { get; }
    public string? ReleaseId { get; init; }
    public string? ReleaseUrl { get; }
    public string? PipelineType { get; }
    public string? PullRequestUrl { get; }
    public string? StageId { get; init; }
    public string CorrelationId { get; }

    private void SetException(Exception? exception)
    {
        if (exception == null)
        {
            return;
        }

        ExceptionType = exception.GetType().Name;
        ExceptionMessage = $"{ExtractFlurlHttpResponse(exception)} Stacktrace: {exception.StackTrace}";
        InnerExceptionType = exception.InnerException?.GetType().Name;
        InnerExceptionMessage = ExtractFlurlHttpResponse(exception.InnerException);
    }

    /// <summary>
    ///     In the case of a FlurlHttpException we want to know the response we got to identify the underlying issue and log
    ///     it.
    ///     This method will become obsolete in case Flurl is phased out.
    /// </summary>
    /// <param name="exception">Any Exception that can potentially be a FlurlHttpException</param>
    /// <returns>A parsed message, which will be more comprehensive in the case of a FlurlHttpRequest</returns>
    private static string? ExtractFlurlHttpResponse(Exception? exception)
    {
        switch (exception)
        {
            case null:
                return null;
            case FlurlHttpException flurlHttpException:
            {
                var unwrappedExceptionMessage = flurlHttpException.GetResponseStringAsync().GetAwaiter().GetResult();

                return unwrappedExceptionMessage == null
                    ? flurlHttpException.Message
                    : $"{flurlHttpException.Message} {unwrappedExceptionMessage}";
            }
            default:
                return exception.Message;
        }
    }
}