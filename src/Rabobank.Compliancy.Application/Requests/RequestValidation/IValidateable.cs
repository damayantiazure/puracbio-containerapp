using FluentValidation.Results;

namespace Rabobank.Compliancy.Application.Requests.RequestValidation;

public interface IValidatable
{
    public ValidationResult Validate();
}