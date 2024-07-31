#nullable enable

using System.Collections;
using Rabobank.Compliancy.Domain.Enums;

namespace Rabobank.Compliancy.Application.Services;

/// <summary>
///     Service definitions to communicate with logAnalytics api.
/// </summary>
public interface ILogIngestionService
{
    /// <summary>
    /// WriteLogEntryAsync will send an entry as a single log entry to Azure Log Analytics.
    /// </summary>
    /// <param name="entry">The object to be send.</param>
    /// <param name="destination">Specify where the logs should be inserted.</param>
    /// <returns>A <see cref="Task" /> that represents an asynchronous operation.</returns>
    Task WriteLogEntryAsync(object entry, LogDestinations destination);

    /// <summary>
    /// WriteLogEntriesAsync will send a collection of entries in a batch to Azure Log Analytics.
    /// </summary>
    /// <param name="entries">The collection of objects</param>
    /// <param name="destination">Specify where the logs should be inserted.</param>
    /// <returns>A <see cref="Task" /> that represents an asynchronous operation.</returns>
    Task WriteLogEntriesAsync(IEnumerable entries, LogDestinations destination);
}