namespace Rabobank.Compliancy.Application.Requests;

public class PipelineRuleRescanRequest : RuleRescanRequestBase
{
    public int PipelineId { get; set; }

    /// <inheritdoc/>
    /// <param name="pipelineId">Represents the ID of the pipeline to be rescanned</param>
    public PipelineRuleRescanRequest(string organization, Guid reportProjectId, int pipelineId, Guid itemProjectId, string ruleName)
        : base(organization, reportProjectId, itemProjectId, ruleName, false)
    {
        PipelineId = pipelineId;
    }

    protected override string GetItemIdAsString()
    {
        return PipelineId.ToString();
    }
}