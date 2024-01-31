namespace Rabobank.Compliancy.Clients.LogAnalyticsClient;

/// <inheritdoc/>
public class LogAnalyticsConfiguration : ILogAnalyticsConfiguration
{
    private const string GrantType = "client_credentials";
    private readonly string _clientId;
    private readonly string _clientSecret;

    public LogAnalyticsConfiguration(string tenantId, string clientId, string clientSecret, string workspaceId, string key)
    {
        TenantId = tenantId;
        _clientId = clientId;
        _clientSecret = clientSecret;
        WorkspaceId = workspaceId;
        Key = key;
    }

    /// <inheritdoc/>
    public string TenantId { get; }

    /// <inheritdoc/>
    public string WorkspaceId { get; }

    /// <inheritdoc/>
    public string Key { get; }

    /// <inheritdoc/>
    public Dictionary<string, string> ContentParameters => new()
    {
        { "grant_type", GrantType },
        { "client_id", _clientId },
        { "client_secret", _clientSecret },
        { "redirect_uri", "http://localhost:3000/login" },
        { "resource", "https://api.loganalytics.io" }
    };
}