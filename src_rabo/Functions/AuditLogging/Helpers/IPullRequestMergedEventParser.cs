using Rabobank.Compliancy.Functions.AuditLogging.Model;

namespace Rabobank.Compliancy.Functions.AuditLogging.Helpers;

public interface IPullRequestMergedEventParser
{
    PullRequestMergedEvent Parse(string json);
}