using Rabobank.Compliancy.Core.Rules.Model;
using System.Collections.Generic;

namespace Rabobank.Compliancy.Core.Rules.Processors;

/// <inheritdoc/>
public class ReconcileProcessor : IReconcileProcessor
{
    private readonly IEnumerable<IReconcile> _itemreconcileRules;
    private readonly IEnumerable<IProjectReconcile> _projectReconcileRules;

    public ReconcileProcessor(
        IEnumerable<IReconcile> itemreconcileRules,
        IEnumerable<IProjectReconcile> projectReconcileRules)
    {
        _itemreconcileRules = itemreconcileRules;
        _projectReconcileRules = projectReconcileRules;
    }

    /// <inheritdoc/>
    public IEnumerable<IReconcile> GetAllItemReconcile()
    {
        return _itemreconcileRules;
    }

    /// <inheritdoc/>
    public IEnumerable<IProjectReconcile> GetAllProjectReconcile()
    {
        return _projectReconcileRules;
    }
}