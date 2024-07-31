using FluentValidation;

namespace Rabobank.Compliancy.Application.Requests.RequestValidation;

public abstract class AbstractRequestBaseValidator<TRequest> : AbstractValidator<TRequest> where TRequest : RequestBase
{
    protected AbstractRequestBaseValidator()
    {
        Include(new RequestBaseValidator());
    }
}