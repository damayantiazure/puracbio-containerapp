using System;
using System.Runtime.Serialization;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Exceptions;

[Serializable]
public class ScanException : Exception
{
    public ScanException()
    {
    }

    public ScanException(string message) : base(message)
    {
    }

    public ScanException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected ScanException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}