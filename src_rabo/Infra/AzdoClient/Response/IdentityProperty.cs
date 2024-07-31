#nullable enable

using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class IdentityProperty
{
    [JsonProperty("$type")]
    public string? Type { get; set; }

    [JsonProperty("$value")]
    public string? Value { get; set; }
}