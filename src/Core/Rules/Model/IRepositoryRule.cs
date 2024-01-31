using Rabobank.Compliancy.Domain.Compliancy;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Core.Rules.Model;

public interface IRepositoryRule : IRule
{
    Task<bool> EvaluateAsync(string organization, string projectId, string repositoryId);
    Task<bool> EvaluateAsync(GitRepo gitRepo);
}