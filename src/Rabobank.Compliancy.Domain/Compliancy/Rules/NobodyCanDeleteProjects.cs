using Rabobank.Compliancy.Domain.Compliancy.Evaluatables.MisUsableEvaluatableTypes;
using System;

namespace Rabobank.Compliancy.Domain.Compliancy.Rules;

public class NobodyCanDeleteProjects : NobodyCanMisuseObject<ProjectMisUse>
{
    protected override IEnumerable<ProjectMisUse> MisUseTypes { get { return new[] { ProjectMisUse.Delete }; } }
}