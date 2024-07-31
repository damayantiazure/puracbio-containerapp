#nullable enable

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Model;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Services;

public interface IPipelineRegistrator
{
    /// <summary>
    ///     Registration of a non prod pipeline. This registration is only stored in table-storage
    /// </summary>
    Task<IActionResult> RegisterNonProdPipelineAsync(
        string organization, string projectId, string pipelineId, string pipelineType, string stageId);

    /// <summary>
    ///     Registration of a production pipeline. This registration is stored in both the CMDB/SM9 and in table-storage
    /// </summary>
    Task<IActionResult> RegisterProdPipelineAsync(string organization, string projectId, string pipelineId,
        string pipelineType, string? userMailAddress, RegistrationRequest input);

    /// <summary>
    ///     Update of a production pipeline. This registration is stored in both the CMDB/SM9 and in table-storage
    /// </summary>
    Task<IActionResult> UpdateProdPipelineRegistrationAsync(string organization, string projectId, string pipelineId,
        string pipelineType, string? userMailAddress, UpdateRequest input);

    /// <summary>
    ///     Update a registration of a non prod pipeline.
    /// </summary>
    Task<IActionResult> UpdateNonProdRegistrationAsync(
        string organization, string projectId, string pipelineId, string pipelineType, string? stageId);

    /// <summary>
    ///     Delete a prod pipeline registration.
    /// </summary>
    Task<IActionResult> DeleteProdPipelineRegistrationAsync(string organization, string projectId, string pipelineId,
        string pipelineType, string? userMailAddress, DeleteRegistrationRequest input);
}