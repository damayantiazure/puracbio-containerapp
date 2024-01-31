#nullable enable

using FluentValidation.Results;
using Rabobank.Compliancy.Application.Requests.RequestValidation;
using Rabobank.Compliancy.Domain.Exceptions;

namespace Rabobank.Compliancy.Application.Requests;

public class RescanPipelineRequest : RequestBase, IValidatable, IExceptionReportConvertible
{
    public int PipelineId { get; set; }

    private readonly RescanPipelineRequestValidator _validator = new();

    public ExceptionReport ToExceptionReport(string functionName, string functionUrl, Exception exception) => 
        RequestExtensions.ToExceptionReport(this, functionName, functionUrl, exception);

    public override ValidationResult Validate() => 
        _validator.Validate(this);
}