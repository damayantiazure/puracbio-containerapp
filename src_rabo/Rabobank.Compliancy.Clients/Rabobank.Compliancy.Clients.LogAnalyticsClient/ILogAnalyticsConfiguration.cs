namespace Rabobank.Compliancy.Clients.LogAnalyticsClient;

/// <summary>
/// An interface that holds the configurations for loganalytics.
/// </summary>
public interface ILogAnalyticsConfiguration
{
    /// <summary>
    /// Getter for the tenant identifier.
    /// </summary>
    string TenantId { get; }

    /// <summary>
    /// Getter for the loganalytics workspace identifier.
    /// </summary>
    string WorkspaceId { get; }

    /// <summary>
    /// Getter for the loganalytics workspace key.
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Getters for the content parameters to be used in the request.
    /// </summary>
    Dictionary<string, string> ContentParameters { get; }
}