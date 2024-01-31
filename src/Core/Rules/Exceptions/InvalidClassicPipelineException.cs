using System;
using System.Runtime.Serialization;

namespace Rabobank.Compliancy.Core.Rules.Exceptions;

[Serializable]
public class InvalidClassicPipelineException : Exception
{
    public InvalidClassicPipelineException()
    {
    }

    public InvalidClassicPipelineException(string message) : base(message)
    {
    }

    public InvalidClassicPipelineException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected InvalidClassicPipelineException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}