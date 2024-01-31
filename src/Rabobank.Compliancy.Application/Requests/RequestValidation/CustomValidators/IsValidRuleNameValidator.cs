using FluentValidation;
using FluentValidation.Validators;
using Rabobank.Compliancy.Domain.Rules;

namespace Rabobank.Compliancy.Application.Requests.RequestValidation.CustomValidators;

public class IsValidRuleNameValidator<T, TProperty> : PropertyValidator<T, TProperty>, IPropertyValidator
{
    private const string _defaultMessage = "'{PropertyName}' is not a valid rule name.";

    /// <inheritdoc/>
    public override string Name => "IsValidRuleNameValidator";

    /// <inheritdoc/>
    public override bool IsValid(ValidationContext<T> context, TProperty value)
    {
        if (value == null)
        {
            return false;
        }

        var ruleNames = typeof(RuleNames).GetFields().Select(x => x.GetRawConstantValue() as string);
        return ruleNames.Any(x => x.Equals($"{value}", StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc/>
    protected override string GetDefaultMessageTemplate(string errorCode)
    {
        return _defaultMessage;
    }
}