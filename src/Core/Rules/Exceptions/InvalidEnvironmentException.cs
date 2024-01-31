using System;
using System.Runtime.Serialization;

namespace Rabobank.Compliancy.Core.Rules.Exceptions;

[Serializable]
public class InvalidEnvironmentException : Exception
{
    public InvalidEnvironmentException()
    {
    }

    public InvalidEnvironmentException(string message) : base(message)
    {
    }

    public InvalidEnvironmentException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected InvalidEnvironmentException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}