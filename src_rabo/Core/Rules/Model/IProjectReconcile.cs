using System.Threading.Tasks;

namespace Rabobank.Compliancy.Core.Rules.Model;

public interface IProjectReconcile
{
    string Name { get; }
    Task ReconcileAsync(string organization, string projectId);
    Task<bool> ReconcileAndEvaluateAsync(string organization, string projectId);
    string[] Impact { get; }
}