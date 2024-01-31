using System.Threading.Tasks;

namespace Rabobank.Compliancy.Core.Rules.Model;

public interface IReconcile
{
    string Name { get; }

    Task ReconcileAsync(string organization, string projectId, string itemId);

    Task<bool> ReconcileAndEvaluateAsync(string organization, string projectId, string itemId);

    string[] Impact { get; }
}