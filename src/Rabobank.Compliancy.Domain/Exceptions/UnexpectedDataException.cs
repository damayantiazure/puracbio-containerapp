using System.Globalization;
using System.Runtime.Serialization;

namespace Rabobank.Compliancy.Domain.Exceptions;

[Serializable]
public class DataNotFoundException : Exception
{
    public DataNotFoundException()
    {
    }

    public DataNotFoundException(string message) : base(message)
    {
    }

    public DataNotFoundException(string message, params object[] stringFormatParameters)
        : base(string.Format(CultureInfo.InvariantCulture, message, stringFormatParameters))
    {
    }

    public DataNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected DataNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}