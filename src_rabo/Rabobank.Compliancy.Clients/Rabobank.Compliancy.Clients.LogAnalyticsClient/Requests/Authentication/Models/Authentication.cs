using Newtonsoft.Json;

namespace Rabobank.Compliancy.Clients.LogAnalyticsClient.Requests.Authentication.Models;

public class Authentication
{
    [JsonProperty("token_type")]
    public string TokenType { get; set; } = default!;

    [JsonProperty("expires_in")]
    public string? ExpiresIn { get; set; }

    [JsonProperty("ext_expires_in")]
    public string? ExtExpiresIn { get; set; }

    [JsonProperty("expires_on")]
    public string? ExpiresOn { get; set; }

    [JsonProperty("not_before")]
    public string? NotBefore { get; set; }

    [JsonProperty("resource")]
    public string? Resource { get; set; }

    [JsonProperty("access_token")]
    public string? AccesToken { get; set; }

    public bool IsExpired
    {
        get
        {
            if (ExpiresOn != null && long.TryParse(ExpiresOn, out long value))
            {
                return DateTimeOffset.FromUnixTimeSeconds(value).CompareTo(DateTime.Now) < 0;
            }

            return false;
        }
    }
}