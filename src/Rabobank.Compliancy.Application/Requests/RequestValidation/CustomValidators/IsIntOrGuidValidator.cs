using FluentValidation;
using FluentValidation.Validators;
using Rabobank.Compliancy.Application.Helpers.Extensions;

namespace Rabobank.Compliancy.Application.Requests.RequestValidation.CustomValidators;

public class IsIntOrGuidValidator<T, TProperty> : PropertyValidator<T, TProperty>, IPropertyValidator
{
    private const string _defaultMessage = "'{PropertyName}' is not a valid integer or guid.";

    /// <inheritdoc/>
    public override string Name => "IsIntOrGuidValidator";

    /// <inheritdoc/>
    public override bool IsValid(ValidationContext<T> context, TProperty value)
    {
        var valStr = value != null ? value.ToString() : string.Empty;
        if (valStr.HasIntegerValue() || valStr.HasGuidValue())
        {
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    protected override string GetDefaultMessageTemplate(string errorCode)
    {
        return _defaultMessage;
    }
}