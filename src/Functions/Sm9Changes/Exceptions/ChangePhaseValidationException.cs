using System;
using System.Runtime.Serialization;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Exceptions;

[Serializable]
public class ChangePhaseValidationException : Exception
{
    public ChangePhaseValidationException()
    {
    }

    public ChangePhaseValidationException(string message) : base(message)
    {
    }

    public ChangePhaseValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected ChangePhaseValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}