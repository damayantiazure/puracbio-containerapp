#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Services;

public interface IAzdoService
{
    Task<bool> IsLowRiskChangeAsync(string organization, string projectId, string runId);
    Task<string?> GetChangeIdFromVariableAsync(string organization, string projectId, string runId);
    Task<IEnumerable<string>?> GetChangeIdsFromTagsAsync(string organization, string projectId, string runId, string regex);
    Task SetTagAsync(string organization, string projectId, string runId, string changeId, string urlHash);
    Task<string?> GetPipelineRunUrlAsync(string organization, string projectId, string runId);
    Task<string?> GetPipelineRunInitiatorAsync(string organization, string projectId, string runId);
}