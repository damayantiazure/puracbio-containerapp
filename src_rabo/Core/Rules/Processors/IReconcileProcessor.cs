using Rabobank.Compliancy.Core.Rules.Model;
using System.Collections.Generic;

namespace Rabobank.Compliancy.Core.Rules.Processors;

/// <summary>
/// Implementations of this interface are responsible of handling all existing reconcile rule Implementations:
/// </summary>
public interface IReconcileProcessor
{
    /// <summary>
    /// Gets a list of item reconcile.
    /// </summary>
    /// <returns>A list of <see cref="IReconcile"/>.</returns>
    IEnumerable<IReconcile> GetAllItemReconcile();

    /// <summary>
    /// Gets a list of project reconcile.
    /// </summary>
    /// <returns>A list of <see cref="IProjectReconcile"/>.</returns>
    IEnumerable<IProjectReconcile> GetAllProjectReconcile();
}