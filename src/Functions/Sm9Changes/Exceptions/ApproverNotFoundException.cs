using System;
using System.Runtime.Serialization;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Exceptions;

[Serializable]
public class ApproverNotFoundException : Exception
{
    public ApproverNotFoundException()
    {
    }

    public ApproverNotFoundException(string message) : base(message)
    {
    }

    public ApproverNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected ApproverNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}