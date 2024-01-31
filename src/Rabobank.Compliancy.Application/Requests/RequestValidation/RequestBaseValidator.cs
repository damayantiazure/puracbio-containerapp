using FluentValidation;

namespace Rabobank.Compliancy.Application.Requests.RequestValidation;

public class RequestBaseValidator : AbstractValidator<RequestBase>
{
    public RequestBaseValidator()
    {
        RuleFor(request => request.Organization)
            .NotNull().NotEmpty();

        RuleFor(request => request.ProjectId)
            .NotEmpty();
    }
}