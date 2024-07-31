using System;
using System.Runtime.Serialization;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Exceptions;

[Serializable]
public class InvalidHeadersException : Exception
{
    public InvalidHeadersException()
    {
    }

    public InvalidHeadersException(string message) : base(message)
    {
    }

    public InvalidHeadersException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected InvalidHeadersException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}