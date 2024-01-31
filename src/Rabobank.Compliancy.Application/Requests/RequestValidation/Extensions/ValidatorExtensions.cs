#nullable enable
using FluentValidation;
using Rabobank.Compliancy.Application.Requests.RequestValidation.CustomValidators;

namespace Rabobank.Compliancy.Application.Requests.RequestValidation.Extensions;

public static class ValidatorExtensions
{
    /// <summary>
    /// Defines a 'is not valid rule name' validator on the current rule builder.
    /// Validation will fail if the property is not a valid rule name
    /// <typeparam name="T">Type of object being validated</typeparam>
    /// <typeparam name="TProperty">Type of property being validated</typeparam>
    /// <param name="ruleBuilder">The rule builder on which the validator should be defined</param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, TProperty> IsValidRuleName<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder)
    {
        return ruleBuilder.SetValidator(new IsValidRuleNameValidator<T, TProperty>());
    }

    /// <summary>
    /// Defines a 'is not integer or guid' validator on the current rule builder.
    /// Validation will fail if the property is not a valid integer or guid
    /// <typeparam name="T">Type of object being validated</typeparam>
    /// <typeparam name="TProperty">Type of property being validated</typeparam>
    /// <param name="ruleBuilder">The rule builder on which the validator should be defined</param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, TProperty> IsValidIntOrGuid<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder)
    {
        return ruleBuilder.SetValidator(new IsIntOrGuidValidator<T, TProperty>());
    }

    /// <summary>
    /// Defines a 'is not integer, guid or dummy' validator on the current rule builder.
    /// Validation will fail if the property is not a valid integer, guid or has the text 'Dummy'
    /// <typeparam name="T">Type of object being validated</typeparam>
    /// <typeparam name="TProperty">Type of property being validated</typeparam>
    /// <param name="ruleBuilder">The rule builder on which the validator should be defined</param>
    public static IRuleBuilderOptions<T, TProperty> IsValidIntGuidOrDummy<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder)
    {
        return ruleBuilder.SetValidator(new IsIntGuidOrDummyValidator<T, TProperty>());
    }

    /// <summary>
    /// Defines a 'is not valid pipeline type' validator on the current rule builder.
    /// Validation will fail if the property is not a valid pipeline type.
    /// <typeparam name="T">Type of object being validated</typeparam>
    /// <typeparam name="TProperty">Type of property being validated</typeparam>
    /// <param name="ruleBuilder">The rule builder on which the validator should be defined</param>
    public static IRuleBuilderOptions<T, TProperty> IsValidPipelineType<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder)
    {
        return ruleBuilder.SetValidator(new IsValidPipelineTypeValidator<T, TProperty>());
    }
}