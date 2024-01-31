#nullable enable

using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class IdentityProperties
{
    [JsonProperty(nameof(Account))]
    public IdentityProperty? Account { get; set; }
}