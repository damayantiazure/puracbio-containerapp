#nullable enable

using Rabobank.Compliancy.Infra.StorageClient.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Infra.StorageClient;

public interface IPipelineRegistrationStorageRepository
{
    Task AddBatchAsync(IEnumerable<PipelineRegistration> items);
    Task ClearAsync();
    Task DeleteEntityAsync(string ciIdentifier, string projectId, string pipelineId, string pipelineType, string stageId);
    Task DeleteEntityAsync(PipelineRegistration? item);
    Task DeleteEntitiesForPipelineAsync(string? ciIdentifier, string projectId, string pipelineId, string pipelineType, string? stageId);
    Task ImportAsync(IEnumerable<PipelineRegistration> items);
    Task<PipelineRegistration?> InsertOrMergeEntityAsync(PipelineRegistration item);
}