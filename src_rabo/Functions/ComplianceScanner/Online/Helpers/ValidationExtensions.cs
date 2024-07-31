using FluentValidation.Results;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Validation;
using System;
using System.Globalization;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Helpers;

public static class ValidationExtensions
{
    private const string ValidationErrorsPrefixText = "Validation errors found in request while executing function";
    private const string InvalidNumberOfValidationErrors = "Cannot convert ValidationResult to exception, there are no errors.";

    public static ValidationErrorsException ToException(this ValidationResult validationResult)
    {
        if (validationResult.Errors.Count == 0)
        {
            throw new InvalidOperationException(InvalidNumberOfValidationErrors);
        }
        return new ValidationErrorsException(validationResult.JoinErrorMessages());
    }

    public static string JoinErrorMessages(this ValidationResult validationResult)
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}",
            /* {0} */ ValidationErrorsPrefixText,
            /* {1} */ Environment.NewLine,
            /* {2} */ validationResult
        );
    }
}