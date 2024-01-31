using System.Runtime.Serialization;

namespace Rabobank.Compliancy.Domain.Exceptions;

[Serializable]
public class InsufficientPermissionsException : Exception
{
    [NonSerialized]
    private const string _defaultMessage = "The Application has Insufficient Permissions to retrieve the necessary objects";

    public InsufficientPermissionsException() : this(_defaultMessage)
    {
    }

    public InsufficientPermissionsException(string message) : base(message)
    {
    }

    public InsufficientPermissionsException(Exception innerException) : this(_defaultMessage, innerException)
    {
    }

    public InsufficientPermissionsException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected InsufficientPermissionsException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}