using System.Runtime.Serialization;

namespace Rabobank.Compliancy.Infra.Sm9Client.Change;

[Serializable]
public class ChangeClientException : Exception
{
    public ChangeClientException()
    {
    }

    public ChangeClientException(string message) : base(message)
    {
    }

    public ChangeClientException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected ChangeClientException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}