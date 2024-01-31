using FluentValidation.Results;
using System;
using System.Runtime.Serialization;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Validation;

[Serializable]
public class ValidationErrorsException : Exception
{
    public readonly ValidationResult ValidationResult;

    public ValidationErrorsException()
    {
    }

    public ValidationErrorsException(string message) : base(message)
    {
    }

    public ValidationErrorsException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected ValidationErrorsException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}