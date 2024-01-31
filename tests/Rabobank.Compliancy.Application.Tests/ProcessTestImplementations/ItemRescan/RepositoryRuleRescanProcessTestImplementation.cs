using Rabobank.Compliancy.Application.ItemRescan;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.Rules.Processors;

namespace Rabobank.Compliancy.Application.Tests.ProcessTestImplementations;

public class RepositoryRuleRescanProcessTestImplementation : RepositoryRuleRescanProcess
{
    public RepositoryRuleRescanProcessTestImplementation(IRuleProcessor ruleProcessor, IProjectService projectService, ICompliancyReportService reportService, IGitRepoService gitRepoService)
        : base(ruleProcessor, projectService, reportService, gitRepoService)
    {
    }
}