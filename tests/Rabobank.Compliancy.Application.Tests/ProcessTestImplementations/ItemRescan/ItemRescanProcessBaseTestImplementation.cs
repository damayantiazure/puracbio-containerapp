using Rabobank.Compliancy.Application.ItemRescan;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.Rules.Processors;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Application.Tests.ProcessTestImplementations;

public class ItemRescanProcessBaseTestImplementation : ItemRescanProcessBase
{
    public ItemRescanProcessBaseTestImplementation(IRuleProcessor ruleProcessor, IProjectService projectService, ICompliancyReportService reportService)
        : base(ruleProcessor, projectService, reportService)
    {
    }

    public async Task<Project> UsesGetParentProject(string organization, Guid projectId)
    {
        return await GetParentProject(organization, projectId);
    }

    public async Task UsesUpdateReportAsync(RuleRescanRequestBase request, bool evaluationResult)
    {
        await UpdateReportAsync(request, evaluationResult);
    }

}