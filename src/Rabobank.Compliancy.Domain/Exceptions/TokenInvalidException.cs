using System.Runtime.Serialization;

namespace Rabobank.Compliancy.Domain.Exceptions;

[Serializable]
public class TokenInvalidException : Exception
{
    public TokenInvalidException()
    {
    }

    public TokenInvalidException(string message) : base(message)
    {
    }

    public TokenInvalidException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected TokenInvalidException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}