using Rabobank.Compliancy.Domain.Compliancy.Evaluatables.MisUsableEvaluatableTypes;
using System;

namespace Rabobank.Compliancy.Domain.Compliancy.Rules;

public class NobodyCanDeleteRepository : NobodyCanMisuseObject<RepositoryMisUse>
{
    protected override IEnumerable<RepositoryMisUse> MisUseTypes { get { return new[] { RepositoryMisUse.Delete, RepositoryMisUse.Manage }; } }
}