using System;
using System.Runtime.Serialization;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Exceptions;

[Serializable]
public class IsProductionItemException : Exception
{
    public IsProductionItemException()
    {
    }

    public IsProductionItemException(string message) : base(message)
    {
    }

    public IsProductionItemException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected IsProductionItemException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}