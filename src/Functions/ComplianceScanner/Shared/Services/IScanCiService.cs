using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.StorageClient.Model;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;

public interface IScanCiService
{
    Task<CiReport> ScanCiAsync(string organization, Project project, string ciIdentifier, DateTime scanDate,
        IEnumerable<PipelineRegistration> pipelineRegistrations);

    Task<NonProdCompliancyReport> ScanNonProdPipelineAsync(string organization, Project project, DateTime scanDate,
        string nonProdPipelineId, IEnumerable<PipelineRegistration> pipelineRegistrations);
}