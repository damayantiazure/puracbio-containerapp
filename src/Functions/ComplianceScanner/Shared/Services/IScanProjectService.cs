using System;
using System.Threading.Tasks;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Infra.AzdoClient.Response;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;

public interface IScanProjectService
{
    Task<CompliancyReport> ScanProjectAsync(
        string organization, Project project, DateTime scanDate, int parallelCiScans);
}