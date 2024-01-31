using System.Runtime.Serialization;

namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb;

[Serializable]
public class CmdbClientException : Exception
{
    public CmdbClientException(string message, Exception exception) : base(message, exception)
    {
    }

    protected CmdbClientException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}