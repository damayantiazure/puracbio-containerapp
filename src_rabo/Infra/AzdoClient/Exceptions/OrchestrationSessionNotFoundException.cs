using System;
using System.Runtime.Serialization;

namespace Rabobank.Compliancy.Infra.AzdoClient.Exceptions;

[Serializable]
public class OrchestrationSessionNotFoundException : Exception
{
    public OrchestrationSessionNotFoundException()
    {
    }

    public OrchestrationSessionNotFoundException(string message) : base(message)
    {
    }

    public OrchestrationSessionNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected OrchestrationSessionNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}