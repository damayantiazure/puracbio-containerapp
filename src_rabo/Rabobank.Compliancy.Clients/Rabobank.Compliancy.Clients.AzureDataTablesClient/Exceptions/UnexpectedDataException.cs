using System.Globalization;
using System.Runtime.Serialization;

namespace Rabobank.Compliancy.Clients.AzureDataTablesClient.Exceptions;

[Serializable]
public class UnexpectedDataException : Exception
{
    public UnexpectedDataException()
    {
    }

    public UnexpectedDataException(string message) : base(message)
    {
    }

    public UnexpectedDataException(string message, params object[] stringFormatParameters)
        : base(string.Format(CultureInfo.InvariantCulture, message, stringFormatParameters))
    {
    }

    public UnexpectedDataException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected UnexpectedDataException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}