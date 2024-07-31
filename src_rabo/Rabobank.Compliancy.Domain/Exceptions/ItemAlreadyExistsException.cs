using System.Globalization;
using System.Runtime.Serialization;

namespace Rabobank.Compliancy.Domain.Exceptions;

[Serializable]
public class ItemAlreadyExistsException : Exception
{
    public ItemAlreadyExistsException()
    {
    }

    public ItemAlreadyExistsException(string message) : base(message)
    {
    }

    public ItemAlreadyExistsException(string message, params object[] stringFormatParameters)
        : base(string.Format(CultureInfo.InvariantCulture, message, stringFormatParameters))
    {
    }

    public ItemAlreadyExistsException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected ItemAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}