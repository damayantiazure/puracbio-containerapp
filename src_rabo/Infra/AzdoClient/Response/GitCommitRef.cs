namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class GitCommitRef
{
    public GitUserDate Author { get; set; }
    public string Comment { get; set; }
    public string CommitId { get; set; }
    public GitUserDate Committer { get; set; }
    public string Url { get; set; }
}