using Rabobank.Compliancy.Application.ItemRescan;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.Rules.Processors;

namespace Rabobank.Compliancy.Application.Tests.ProcessTestImplementations;

public class ProjectRuleRescanProcessTestImplementation : ProjectRuleRescanProcess
{
    public ProjectRuleRescanProcessTestImplementation(IRuleProcessor ruleProcessor, IProjectService projectService, ICompliancyReportService reportService)
        : base(ruleProcessor, projectService, reportService)
    {
    }
}