using FluentValidation;
using Rabobank.Compliancy.Application.Requests.RequestValidation.Extensions;

namespace Rabobank.Compliancy.Application.Requests.RequestValidation;

public class ReconcileRequestValidator : AbstractRequestBaseValidator<ReconcileRequest>
{
    public ReconcileRequestValidator()
    {
        RuleFor(request => request.RuleName)
            .NotNull().NotEmpty().IsValidRuleName();

        RuleFor(request => request.ItemId)
            .NotNull().NotEmpty().IsValidIntOrGuid();
    }
}