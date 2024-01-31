#nullable enable

using FluentValidation.Results;
using Rabobank.Compliancy.Application.Requests.RequestValidation;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Exceptions;

namespace Rabobank.Compliancy.Application.Requests;

public class RegisterDeviationRequest : RequestBase, IValidatable, IExceptionReportConvertible
{
    private readonly RegisterDeviationRequestValidator _validator = new();

    public string? CiIdentifier { get; set; }
    public string? RuleName { get; set; }
    public string? ItemId { get; set; }
    public Guid? ForeignProjectId { get; set; }
    public string? Comment { get; set; }
    public DeviationReason? Reason { get; set; }
    public DeviationApplicabilityReason? ReasonNotApplicable { get; set; }
    public string? ReasonNotApplicableOther { get; set; }
    public string? ReasonOther { get; set; }

    public ExceptionReport ToExceptionReport(string functionName, string functionUrl, Exception exception) =>
        RequestExtensions.ToExceptionReport(this, functionName, functionUrl, exception);

    public override ValidationResult Validate() => _validator.Validate(this);
}