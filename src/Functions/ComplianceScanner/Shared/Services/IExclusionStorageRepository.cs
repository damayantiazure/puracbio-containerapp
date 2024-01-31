#nullable enable

using Microsoft.AspNetCore.Mvc;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;

public interface IExclusionStorageRepository
{
    Task<Exclusion?> GetExclusionAsync(PipelineRunInfo runInfo);
    Task<IActionResult> CreateExclusionAsync(HttpRequestMessage request, PipelineRunInfo runInfo);
    Task<IActionResult> UpdateExclusionAsync(HttpRequestMessage request, PipelineRunInfo runInfo);
    Task SetRunIdAsync(PipelineRunInfo? runInfo);
}