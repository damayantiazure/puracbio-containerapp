using System;
using System.Runtime.Serialization;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Exceptions;

[Serializable]
public class InvalidUserInputException : Exception
{
    public InvalidUserInputException()
    {
    }

    public InvalidUserInputException(string message) : base(message)
    {
    }

    public InvalidUserInputException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected InvalidUserInputException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}