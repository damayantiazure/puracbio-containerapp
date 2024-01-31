using Rabobank.Compliancy.Application.Requests;

namespace Rabobank.Compliancy.Application.Interfaces.Reconcile;

/// <summary>
/// An interface defining the logic for the reconcile process.
/// </summary>
public interface IReconcileBase
{
    /// <summary>
    /// ReconcileAsync will handle the reconcile process of a specific rule.
    /// </summary>
    /// <param name="reconcileRequest">The reconcile request.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Represents an asynchronous operation as <see cref="Task"/>.</returns>
    Task ReconcileAsync(ReconcileRequest reconcileRequest, CancellationToken cancellationToken);

    /// <summary>
    /// Checks for a specific rule name in the reconcile process.
    /// </summary>
    /// <param name="ruleName">The rule name to be checked.</param>
    /// <returns>Returns a <see cref="bool"/> wether the reconcile process contains a rulename.</returns>
    bool HasRuleName(string ruleName);
}