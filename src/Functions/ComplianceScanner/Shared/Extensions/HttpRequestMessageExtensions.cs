using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Extensions;

public static class HttpRequestMessageExtensions
{
    public static async Task<T> DeserializeContentAsync<T>(this HttpRequestMessage httpRequestMessage)
    {
        if (httpRequestMessage.Content == null)
        {
            return default;
        }

        var content = await httpRequestMessage.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(content);
    }
}