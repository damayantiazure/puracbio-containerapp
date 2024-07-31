using System.Net;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.HttpClientExtensions;

public class BinaryMediaTypeFormatter : MediaTypeFormatter
{

    private readonly Type _supportedType = typeof(byte[]);

    public BinaryMediaTypeFormatter() : this(false)
    {
    }

    public BinaryMediaTypeFormatter(bool isAsync)
    {
        SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/octet-stream"));
        IsAsync = isAsync;
    }

    public bool IsAsync { get; set; } = false;

    public override bool CanReadType(Type type)
    {
        return type == _supportedType;
    }

    public override bool CanWriteType(Type type)
    {
        return type == _supportedType;
    }

    public override Task<object> ReadFromStreamAsync(Type type, Stream readStream,
        HttpContent content, IFormatterLogger formatterLogger)
    {
        var readTask = GetReadTask(readStream);
        if (IsAsync)
        {
            readTask.Start();
        }
        else
        {
            readTask.RunSynchronously();
        }
        return readTask;
    }

    public override Task WriteToStreamAsync(Type type, object value, Stream writeStream,
        HttpContent content, TransportContext transportContext)
    {
        value ??= Array.Empty<byte>();
        var writeTask = GetWriteTask(writeStream, (byte[])value);
        if (IsAsync)
        {
            writeTask.Start();
        }
        else
        {
            writeTask.RunSynchronously();
        }
        return writeTask;
    }

    private static Task<object> GetReadTask(Stream stream)
    {
        return new Task<object>(() =>
        {
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        });
    }

    private static Task GetWriteTask(Stream stream, byte[] data)
    {
        return new Task(() =>
        {
            var ms = new MemoryStream(data);
            ms.CopyTo(stream);
        });
    }
}