namespace Rabobank.Compliancy.Application.Requests;

public class RepositoryRuleRescanRequest : RuleRescanRequestBase
{
    public Guid GitRepoId { get; set; }

    /// <inheritdoc/>
    /// <param name="gitRepoId">Represents the ID of the pipeline to be rescanned</param>
    public RepositoryRuleRescanRequest(string organization, Guid reportProjectId, Guid gitRepoId, Guid gitRepoProjectId, string ruleName)
        : base(organization, reportProjectId, gitRepoProjectId, ruleName, false)
    {
        GitRepoId = gitRepoId;
    }

    protected override string GetItemIdAsString()
    {
        return GitRepoId.ToString();
    }
}