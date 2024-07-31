#nullable enable

using System.Collections;
using AutoMapper;
using Azure.Core;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Infrastructure.Dto.Logging;
using Rabobank.Compliancy.Infrastructure.Extensions;
using Rabobank.Compliancy.Infrastructure.InternalContracts;

namespace Rabobank.Compliancy.Infrastructure;

/// <inheritdoc />
public class LogIngestionService : ILogIngestionService
{
    private const string _dtoClassNameSuffix = "Dto";
    private readonly IIngestionClientFactory _ingestionClientFactory;
    private readonly IMapper _mapper;

    public LogIngestionService(IIngestionClientFactory ingestionClientFactory, IMapper mapper)
    {
        _ingestionClientFactory = ingestionClientFactory;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public Task WriteLogEntryAsync(object entry, LogDestinations destination)
    {
        var logEntry = MapToLogModel(entry, destination);
        var clientFactoryResult = _ingestionClientFactory.Create(logEntry.GetType().Name);
        var requestContent = RequestContent.Create(logEntry
            .ReconstructForIngestion()
            .ToBinaryData());

        return clientFactoryResult.Client.UploadAsync(
            clientFactoryResult.ClientConfig.RuleId,
            clientFactoryResult.ClientConfig.StreamName,
            requestContent);
    }

    /// <inheritdoc />
    public Task WriteLogEntriesAsync(IEnumerable entries, LogDestinations destination)
    {
        Type? logElementType = null;

        var logEntries = new List<LogModelDtoBase>();
        foreach (var entry in entries)
        {
            var logEntry = MapToLogModel(entry, destination);
            logElementType ??= logEntry.GetType();
            logEntries.Add(logEntry);
        }

        if (logElementType == null || !logEntries.Any())
        {
            return Task.CompletedTask;
        }

        var clientFactoryResult = _ingestionClientFactory.Create(logElementType.Name);
        var requestContent = RequestContent.Create(logEntries
            .ReconstructForIngestion()
            .ToBinaryData());
        
        return clientFactoryResult.Client.UploadAsync(
            clientFactoryResult.ClientConfig.RuleId,
            clientFactoryResult.ClientConfig.StreamName,
            requestContent);
    }

    private LogModelDtoBase MapToLogModel(object entry, LogDestinations destination)
    {
        var destinationType = typeof(LogModelDtoBase).Assembly.GetType(
            $"{typeof(LogModelDtoBase).Namespace}.{destination}{_dtoClassNameSuffix}", true);
        var logEntry = (LogModelDtoBase)_mapper.Map(entry, entry.GetType(), destinationType);
        logEntry.TimeGenerated = DateTime.UtcNow;
        return logEntry;
    }
}