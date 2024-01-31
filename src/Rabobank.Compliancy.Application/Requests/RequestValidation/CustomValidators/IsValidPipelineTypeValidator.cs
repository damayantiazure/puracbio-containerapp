#nullable enable
using FluentValidation;
using FluentValidation.Validators;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Application.Requests.RequestValidation.CustomValidators;

public class IsValidPipelineTypeValidator<T, TProperty> : PropertyValidator<T, TProperty>, IPropertyValidator
{
    private const string _defaultMessage = "'{PropertyName}' is not a valid pipeline type.";
    private readonly IList<string> PipelineTypes = new List<string> { PipelineReleaseType.YamlRelease, PipelineReleaseType.ClassicRelease };

    /// <inheritdoc/>

    public override string Name => "IsValidRuleNameValidator";

    /// <inheritdoc/>
    public override bool IsValid(ValidationContext<T> context, TProperty value)
    {
        if (value == null)
        {
            return false;
        }

        return PipelineTypes.Any(t => string.Equals(t, value.ToString(), StringComparison.InvariantCultureIgnoreCase));
    }

    /// <inheritdoc/>
    protected override string GetDefaultMessageTemplate(string errorCode)
    {
        return _defaultMessage;
    }
}