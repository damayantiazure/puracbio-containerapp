using System;
using System.Collections.Generic;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;

public interface IVerifyComplianceService
{
    public IEnumerable<PrincipleReport> CreatePrincipleReports(
        IEnumerable<EvaluatedRule> evaluatedRules, DateTime scanDate);
}