using Newtonsoft.Json;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Models;

public abstract class ExtensionData
{
    [JsonProperty("id")]
    public string? Id { get; set; }

    [JsonProperty("__etag")]
    public int Etag { get; set; } = -1;
}