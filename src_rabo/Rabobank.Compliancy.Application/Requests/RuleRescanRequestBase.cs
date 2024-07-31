namespace Rabobank.Compliancy.Application.Requests;

/// <summary>
/// Base class for a RuleRescan request covering all common 
/// </summary>
public abstract class RuleRescanRequestBase
{
    public string Organization { get; set; }
    public Guid ReportProjectId { get; set; }
    public Guid ItemProjectId { get; set; }
    public string RuleName { get; set; }
    /// <summary>
    /// Property that represets the Generic form of an Item Id in string format
    /// </summary>
    public string ItemIdAsString { get => GetItemIdAsString(); }
    public bool ConcernsProjectRule { get; internal set; }

    /// <param name="organization">Represents the organization the report and items live in</param>
    /// <param name="reportProjectId">Represents the Project ID of the project the report lives in</param>
    /// <param name="itemProjectId">Represents the Project ID of the project the item that is to-be-rescanned lives in</param>
    /// <param name="ruleName">Represents the name of the rule that the item needs to be rescanned for</param>
    /// <param name="concernsProjectRule">Represents if this concerns a project rule. This parameter is used for the Report Update process, as a projectRule would need to be updated differently</param>
    protected RuleRescanRequestBase(string organization, Guid reportProjectId, Guid itemProjectId, string ruleName, bool concernsProjectRule)
    {
        Organization = organization;
        ReportProjectId = reportProjectId;
        ItemProjectId = itemProjectId;
        RuleName = ruleName;
        ConcernsProjectRule = concernsProjectRule;
    }

    protected abstract string GetItemIdAsString();
}