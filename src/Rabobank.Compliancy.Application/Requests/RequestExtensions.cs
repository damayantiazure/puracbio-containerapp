#nullable enable

using Rabobank.Compliancy.Domain.Exceptions;

namespace Rabobank.Compliancy.Application.Requests;

public static class RequestExtensions
{
    public static ExceptionReport ToExceptionReport(this RegisterDeviationRequest registerDeviationRequest,
        string functionName, string functionUrl, Exception ex) => new(ex)
        {
            /* Function variables */
            FunctionName = functionName,
            RequestUrl = functionUrl,

            /* Request variables */
            Organization = registerDeviationRequest.Organization,
            ProjectId = registerDeviationRequest.ProjectId.ToString(),
            CiIdentifier = registerDeviationRequest.CiIdentifier,
            ItemId = registerDeviationRequest.ItemId,
            RuleName = registerDeviationRequest.RuleName
        };

    public static ExceptionReport ToExceptionReport(this RescanPipelineRequest rescanPipelineRequest,
        string functionName, string functionUrl, Exception ex) => new(ex)
        {
            /* Function variables */
            FunctionName = functionName,
            RequestUrl = functionUrl,

            /* Request variables */
            Organization = rescanPipelineRequest.Organization,
            ProjectId = rescanPipelineRequest.ProjectId.ToString(),
            ItemId = rescanPipelineRequest.PipelineId.ToString()
        };
}