#nullable enable

using System.Net;
using AutoMapper;
using Azure;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Clients.AzureDataTablesClient;
using Rabobank.Compliancy.Clients.AzureDataTablesClient.Deviations;
using Rabobank.Compliancy.Clients.AzureQueueClient;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Deviations;
using Rabobank.Compliancy.Infrastructure.Dto.Queue;
using Rabobank.Compliancy.Infrastructure.Extensions;
using TableStorage.Abstractions.Store;

namespace Rabobank.Compliancy.Infrastructure;

/// <inheritdoc />
public class DeviationService : IDeviationService
{
    private readonly Lazy<ITableStore<DeviationEntity>> _lazyRepository;
    private readonly IMapper _mapper;
    private readonly IQueueClientFacade _queueClient;

    /// <summary>
    ///     Constructor intended only for dependency injection
    /// </summary>
    /// <param name="factory">
    ///     Injected using the
    ///     <see href="https://github.com/Tazmainiandevil/TableStorage.Abstractions">TableStorage.Abstractions</see> package
    /// </param>
    /// <param name="queueClient">Injected queueClient for sending messages to the queue"</param>
    /// <param name="mapper">Injected autoMapper</param>
    public DeviationService(Func<ITableStore<DeviationEntity>> factory, IQueueClientFacade queueClient, IMapper mapper)
    {
        _lazyRepository = new Lazy<ITableStore<DeviationEntity>>(factory);
        _queueClient = queueClient;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task CreateOrReplaceDeviationAsync(Deviation deviation, string username,
        CancellationToken cancellationToken = default)
    {
        var deviationEntity = deviation.ToEntity(username);
        await _lazyRepository.Value.InsertOrReplaceAsync(deviationEntity, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Deviation?> GetDeviationAsync(Project project, string? ruleName, string? itemId,
        string? ciIdentifier, Guid? foreignProjectId, CancellationToken cancellationToken = default)
    {
        if (ruleName == null)
        {
            throw new ArgumentNullException(nameof(ruleName));
        }

        if (itemId == null)
        {
            throw new ArgumentNullException(nameof(itemId));
        }

        if (ciIdentifier == null)
        {
            throw new ArgumentNullException(nameof(ciIdentifier));
        }

        return GetDeviationInternalAsync(project, ruleName, itemId, ciIdentifier, foreignProjectId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Deviation>> GetDeviationsAsync(Guid projectId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var deviationEntities =
                await _lazyRepository.Value.GetByPartitionKeyAsync(projectId.ToString(), cancellationToken);
            if (deviationEntities == null)
            {
                return Array.Empty<Deviation>();
            }

            return deviationEntities
                .Where(e => e != null)
                .Select(e => e.ToDeviation()!);
        }
        catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
        {
            return Array.Empty<Deviation>();
        }
    }

    /// <inheritdoc />
    public Task DeleteDeviationAsync(Deviation deviation, CancellationToken cancellationToken = default)
    {
        var deviationEntity = deviation.ToDeleteEntity();
        return _lazyRepository.Value.DeleteAsync(deviationEntity, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SendDeviationUpdateRecord(Deviation deviation, DeviationReportLogRecordType recordType)
    {
        var deviationEntity = _mapper.Map<DeviationQueueDto>(deviation);
        deviationEntity.RecordType = recordType.ToString();
        await _queueClient.SendMessageAsync(deviationEntity);
    }

    private async Task<Deviation?> GetDeviationInternalAsync(Project project, string? ruleName, string? itemId,
        string? ciIdentifier, Guid? foreignProjectId, CancellationToken cancellationToken)
    {
        var rowKey = RowKeyGenerator.GenerateRowKey(project.Organization, project.Id, ruleName, itemId, ciIdentifier,
            foreignProjectId);
        try
        {
            var deviationEntity =
                await _lazyRepository.Value.GetRecordAsync(project.Id.ToString(), rowKey, cancellationToken);

            return deviationEntity.ToDeviation();
        }
        catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}