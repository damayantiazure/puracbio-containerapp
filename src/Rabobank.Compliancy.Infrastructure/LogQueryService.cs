#nullable enable

using Azure.Monitor.Query;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Domain.Monitoring;
using Rabobank.Compliancy.Infrastructure.Config;
using Rabobank.Compliancy.Infrastructure.Extensions;

namespace Rabobank.Compliancy.Infrastructure;

/// <inheritdoc />
public class LogQueryService : ILogQueryService
{
    private readonly LogConfig _logConfig;
    private readonly LogsQueryClient _logsQueryClient;

    public LogQueryService(LogsQueryClient logsQueryClient, LogConfig logConfig)
    {
        _logsQueryClient = logsQueryClient;
        _logConfig = logConfig;
    }

    /// <inheritdoc />
    public async Task<T?> GetQueryEntryAsync<T>(string kustoQuery, CancellationToken cancellationToken = default)
        where T : class, new()
    {
        var unionKustoQuery = kustoQuery.ToWildCardUnion();

        var result = await _logsQueryClient.QueryWorkspaceAsync(_logConfig.WorkspaceId, unionKustoQuery,
            QueryTimeRange.All, null, cancellationToken);

        return result.Value.ToGenericObject<T>();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<T>> GetQueryEntriesAsync<T>(string kustoQuery,
        CancellationToken cancellationToken = default) where T : class, new()
    {
        var unionKustoQuery = kustoQuery.ToWildCardUnion();

        var result = await _logsQueryClient.QueryWorkspaceAsync(_logConfig.WorkspaceId, unionKustoQuery,
            QueryTimeRange.All, null, cancellationToken);

        return result.HasValue
            ? result.Value.ToGenericCollection<T>()
            : throw new SourceItemNotFoundException();
    }
}