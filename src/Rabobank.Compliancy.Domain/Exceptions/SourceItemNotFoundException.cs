using System.Globalization;
using System.Runtime.Serialization;

namespace Rabobank.Compliancy.Domain.Exceptions;

[Serializable]
public class SourceItemNotFoundException : Exception
{
    [NonSerialized]
    private const string _defaultMessage = "A valid Item could not be found while querying an external resource";

    public SourceItemNotFoundException() : this(_defaultMessage)
    {
    }

    public SourceItemNotFoundException(string message) : base(message)
    {
    }

    public SourceItemNotFoundException(Exception innerException) : this(_defaultMessage, innerException)
    {
    }

    public SourceItemNotFoundException(string message, params object[] stringFormatParameters)
        : base(string.Format(CultureInfo.InvariantCulture, message, stringFormatParameters))
    {
    }

    public SourceItemNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public SourceItemNotFoundException(string message, Exception innerException, params object[] stringFormatParameters)
        : base(string.Format(CultureInfo.InvariantCulture, message, stringFormatParameters), innerException)
    {
    }

    protected SourceItemNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}