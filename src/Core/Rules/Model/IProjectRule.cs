using Rabobank.Compliancy.Domain.Compliancy;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Core.Rules.Model;

public interface IProjectRule : IRule
{
    Task<bool> EvaluateAsync(string organization, string projectId);
    Task<bool> EvaluateAsync(Project project);
}