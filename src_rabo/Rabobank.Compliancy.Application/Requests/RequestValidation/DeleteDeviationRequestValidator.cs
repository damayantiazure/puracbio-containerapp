#nullable enable
using FluentValidation;
using Rabobank.Compliancy.Application.Requests.RequestValidation.Extensions;

namespace Rabobank.Compliancy.Application.Requests.RequestValidation;

public class DeleteDeviationRequestValidator : AbstractRequestBaseValidator<DeleteDeviationRequest>
{
    private const string ConfigurationItemRegexPattern = @"\bCI\d{7}\b";

    public DeleteDeviationRequestValidator()
    {
        RuleFor(request => request.CiIdentifier)
           .NotNull().NotEmpty().Matches(ConfigurationItemRegexPattern);

        RuleFor(request => request.ItemId)
           .NotNull().IsValidIntGuidOrDummy();

        RuleFor(request => request.RuleName)
            .NotNull().NotEmpty().IsValidRuleName();

        RuleFor(request => request.ForeignProjectId)
            .IsValidIntOrGuid().When(x => x.ForeignProjectId.HasValue);
    }
}