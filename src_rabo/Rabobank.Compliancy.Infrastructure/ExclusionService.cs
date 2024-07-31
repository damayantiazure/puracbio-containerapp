#nullable enable

using System.Net;
using AutoMapper;
using Azure;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Clients.AzureDataTablesClient;
using Rabobank.Compliancy.Clients.AzureDataTablesClient.Exclusions;
using Rabobank.Compliancy.Domain.Compliancy.Exclusions;
using TableStorage.Abstractions.Store;

namespace Rabobank.Compliancy.Infrastructure;

/// <inheritdoc />
public class ExclusionService : IExclusionService
{
    private const string _partitionKey = "Exclusion";
    private readonly Lazy<ITableStore<ExclusionEntity>> _lazyRepository;
    private readonly IMapper _mapper;

    public ExclusionService(Func<ITableStore<ExclusionEntity>> factory, IMapper mapper)
    {
        _lazyRepository = new Lazy<ITableStore<ExclusionEntity>>(factory);
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<Exclusion?> CreateOrUpdateExclusionAsync(Exclusion exclusion,
        CancellationToken cancellationToken = default)
    {
        var exclusionEntity = _mapper.Map<ExclusionEntity>(exclusion);
        await _lazyRepository.Value.InsertOrReplaceAsync(exclusionEntity, cancellationToken);

        return await GetExclusionInternalAsync(exclusion.Organization, exclusion.ProjectId, exclusion.PipelineId,
            exclusion.PipelineType, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Exclusion?> GetExclusionAsync(string? organization, Guid? projectId, int? pipelineId,
        string? pipelineType, CancellationToken cancellationToken = default) =>
        await GetExclusionInternalAsync(organization, projectId, pipelineId.ToString(), pipelineType,
            cancellationToken);

    private async Task<Exclusion?> GetExclusionInternalAsync(string? organization, Guid? projectId, string? pipelineId,
        string? pipelineType, CancellationToken cancellationToken = default)
    {
        var rowKey = RowKeyGenerator.GenerateRowKey(organization, projectId, pipelineId, pipelineType);
        try
        {
            var exclusionEntity = await _lazyRepository.Value.GetRecordAsync(_partitionKey, rowKey, cancellationToken);
            return _mapper.Map<Exclusion>(exclusionEntity);
        }
        catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}