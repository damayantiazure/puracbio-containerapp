#nullable enable

namespace Rabobank.Compliancy.Application.Services;

/// <summary>
///     Service definitions to communicate with logAnalytics api.
/// </summary>
public interface ILogQueryService
{
    /// <summary>
    ///     GetQueryEntryAsync will perform a kusto query language (KQL) on the specified logAnalytics workspace.
    /// </summary>
    /// <typeparam name="TClass">The entity type.</typeparam>
    /// <param name="kustoQuery">The kusto query language that is presented as a <see cref="string" />.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>An instance of the specified domain class.</returns>
    Task<TClass?> GetQueryEntryAsync<TClass>(string kustoQuery, CancellationToken cancellationToken = default)
        where TClass : class, new();

    /// <summary>
    ///     GetQueryEntriesAsync will perform a kusto query language (KQL) on the specified logAnalytics workspace.
    /// </summary>
    /// <typeparam name="TClass">The entity type.</typeparam>
    /// <param name="kustoQuery">The kusto query language that is presented as a <see cref="string" />.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>An instance of the specified domain class.</returns>
    Task<IEnumerable<TClass>> GetQueryEntriesAsync<TClass>(string kustoQuery,
        CancellationToken cancellationToken = default) where TClass : class, new();
}