namespace Rabobank.Compliancy.Application.Requests;

public class ProjectRuleRescanRequest : RuleRescanRequestBase
{
    /// <inheritdoc/>
    public ProjectRuleRescanRequest(string organization, Guid reportProjectId, Guid scannableProjectId, string ruleName)
        : base(organization, reportProjectId, scannableProjectId, ruleName, true)
    {
    }

    protected override string GetItemIdAsString()
    {
        return ItemProjectId.ToString();
    }
}