#nullable enable

using System.Globalization;
using System.Runtime.Serialization;

namespace Rabobank.Compliancy.Domain.Exceptions;

[Serializable]
public class ExclusionApproverAlreadyExistsException : Exception
{
    private const string _alreadyApproved =
    @"There already is a valid exclusion for this pipeline.";

    public ExclusionApproverAlreadyExistsException() : base(_alreadyApproved)
    {
    }

    public ExclusionApproverAlreadyExistsException(string message) : base(message)
    {
    }

    public ExclusionApproverAlreadyExistsException(string message, params object[] stringFormatParameters)
        : base(string.Format(CultureInfo.InvariantCulture, message, stringFormatParameters))
    {
    }

    public ExclusionApproverAlreadyExistsException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected ExclusionApproverAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}