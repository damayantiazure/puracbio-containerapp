using System;
using System.Runtime.Serialization;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Exceptions;

[Serializable]
public class ChangeIdNotFoundException : Exception
{
    public ChangeIdNotFoundException()
    {
    }

    public ChangeIdNotFoundException(string message) : base(message)
    {
    }

    public ChangeIdNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected ChangeIdNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}