#nullable enable
using FluentValidation;
using FluentValidation.Validators;
using Rabobank.Compliancy.Application.Helpers.Extensions;

namespace Rabobank.Compliancy.Application.Requests.RequestValidation.CustomValidators;

public class IsIntGuidOrDummyValidator<T, TProperty> : PropertyValidator<T, TProperty>, IPropertyValidator
{
    private const string DefaultMessage = "'{PropertyName}' is not a valid integer, guid or default.";
    private const string DummyItemIdValue = "dummy";

    /// <inheritdoc/>
    public override string Name => "IsIntGuidOrDefaultValidator";

    /// <inheritdoc/>
    public override bool IsValid(ValidationContext<T> context, TProperty value)
    {
        var propertyValue = value as string;

        // The value can in some scenarios, contains a the text 'Dummy'.
        return propertyValue != null && (propertyValue.HasIntegerValue() || propertyValue.HasGuidValue()
            || propertyValue.Equals(DummyItemIdValue, StringComparison.InvariantCultureIgnoreCase));
    }

    /// <inheritdoc/>
    protected override string GetDefaultMessageTemplate(string errorCode) => DefaultMessage;
}