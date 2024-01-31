using System;
using System.Runtime.Serialization;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Exceptions;

[Serializable]
public class InitiatorNotFoundException : Exception
{
    public InitiatorNotFoundException()
    {
    }

    public InitiatorNotFoundException(string message) : base(message)
    {
    }

    public InitiatorNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected InitiatorNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}