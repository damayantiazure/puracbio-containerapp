using System;
using System.Runtime.Serialization;

namespace Rabobank.Compliancy.Infra.StorageClient.Exceptions;

[Serializable]
public class NoRegisteredStagesFoundException : Exception
{
    public NoRegisteredStagesFoundException()
    {
    }

    public NoRegisteredStagesFoundException(string message) : base(message)
    {
    }

    public NoRegisteredStagesFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected NoRegisteredStagesFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}