#nullable enable

using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Deviations;
using Rabobank.Compliancy.Domain.Compliancy.Reports;

namespace Rabobank.Compliancy.Application.Services;

/// <summary>
///     Service definitions to manage the compliancy report.
/// </summary>
public interface ICompliancyReportService
{
    /// <summary>
    ///     Add the deviations from the cloud storage to the compliancy report.
    /// </summary>
    Task UpdateComplianceReportAsync(string organization, Guid projectId,
        CompliancyReport compliancyReport, DateTime scanDate);

    /// <summary>
    ///     Replace an existing CiReport in a compliancy report with a new one for a given project.
    /// </summary>
    Task UpdateCiReportAsync(string organization, Guid projectId, string projectName, CiReport newCiReport, DateTime scanDate);

    /// <summary>
    ///     Replace an existing NonProdCompliancyReport in a compliancy report with a new one for a given project.
    /// </summary>
    Task UpdateNonProdPipelineReportAsync(string organization, string projectName, NonProdCompliancyReport nonProdCompliancyReport);

    /// <summary>
    ///     Update the compliancy status for a specific rule in the compliancy report for a given project.
    /// </summary>
    Task UpdateComplianceStatusAsync(Project project, Guid itemProjectId, string itemId, string ruleName,
        bool isProjectRule, bool isCompliant);

    /// <summary>
    ///     Add a deviation to a compliancy report.
    /// </summary>
    Task AddDeviationToReportAsync(Deviation? deviation);

    /// <summary>
    ///     Removes deviation from the compliancy report.
    /// </summary>
    Task RemoveDeviationFromReportAsync(Deviation deviation);

    /// <summary>
    ///     Update a pipeline registration in the compliancy report.
    /// </summary>
    Task UpdateRegistrationAsync(
        string organization, string projectName, string pipelineId, string pipelineType, string? ciIdentifier);

    /// <summary>
    /// UnRegisteredPipelineAsync will remove the pipeline registration from the compliancy report.
    /// </summary>
    Task UnRegisteredPipelineAsync(string organization, string projectName, string pipelineId, string pipelineType, CancellationToken cancellationToken = default);
}