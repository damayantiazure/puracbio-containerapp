using System;
using System.Runtime.Serialization;

namespace Rabobank.Compliancy.Core.Rules.Exceptions;

[Serializable]
public class InvalidYamlPipelineException : Exception
{
    public InvalidYamlPipelineException()
    {
    }

    public InvalidYamlPipelineException(string message) : base(message)
    {
    }

    public InvalidYamlPipelineException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected InvalidYamlPipelineException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}