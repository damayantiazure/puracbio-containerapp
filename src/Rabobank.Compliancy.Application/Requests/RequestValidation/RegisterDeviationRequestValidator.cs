#nullable enable
using FluentValidation;
using Rabobank.Compliancy.Application.Requests.RequestValidation.Extensions;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Application.Requests.RequestValidation;

public class RegisterDeviationRequestValidator : AbstractRequestBaseValidator<RegisterDeviationRequest>
{
    public RegisterDeviationRequestValidator()
    {
        RuleFor(request => request.CiIdentifier)
             .NotEmpty();

        RuleFor(request => request.RuleName)
            .NotEmpty().IsValidRuleName();

        RuleFor(request => request.ItemId)
            .NotEmpty().IsValidIntGuidOrDummy();

        RuleFor(request => request.Reason)
            .NotEmpty();

        RuleFor(request => request.ReasonNotApplicable)
            .NotEmpty()
            .When(request => request.Reason == DeviationReason.RuleNotApplicable);

        RuleFor(request => request.ReasonOther)
            .NotEmpty()
            .When(request => request.Reason == DeviationReason.Other);

        RuleFor(request => request.ReasonNotApplicableOther)
            .NotEmpty()
            .When(request => request.ReasonNotApplicable == DeviationApplicabilityReason.Other);

        RuleFor(request => request.Comment)
            .NotEmpty();
    }
}