using System;
using System.Runtime.Serialization;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Exceptions;

[Serializable]
public class PipelineUrlNotFoundException : Exception
{
    public PipelineUrlNotFoundException()
    {
    }

    public PipelineUrlNotFoundException(string message) : base(message)
    {
    }

    public PipelineUrlNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected PipelineUrlNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}