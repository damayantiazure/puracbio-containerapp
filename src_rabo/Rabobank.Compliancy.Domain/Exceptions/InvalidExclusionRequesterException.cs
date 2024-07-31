#nullable enable

using System.Globalization;
using System.Runtime.Serialization;

namespace Rabobank.Compliancy.Domain.Exceptions;

[Serializable]
public class InvalidExclusionRequesterException : Exception
{
    private const string InvalidApprover =
           @"The approval of your exclusion request failed.
The approver is the same person as the requester.
In order to request a valid exclusion, 4-eyes is required.";

    public InvalidExclusionRequesterException() : base(InvalidApprover)
    {
    }

    public InvalidExclusionRequesterException(string message) : base(message)
    {
    }

    public InvalidExclusionRequesterException(string message, params object[] stringFormatParameters)
        : base(string.Format(CultureInfo.InvariantCulture, message, stringFormatParameters))
    {
    }

    public InvalidExclusionRequesterException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected InvalidExclusionRequesterException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}