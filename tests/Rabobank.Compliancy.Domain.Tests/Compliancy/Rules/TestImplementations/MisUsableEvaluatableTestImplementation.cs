using Rabobank.Compliancy.Domain.Compliancy.Authorizations;
using Rabobank.Compliancy.Domain.Compliancy.Evaluatables;
using Rabobank.Compliancy.Domain.Compliancy.Evaluatables.MisUsableEvaluatableTypes;
using System;

namespace Rabobank.Compliancy.Domain.Tests.Compliancy.Rules.TestImplementations;

public class MisUsableEvaluatableTestImplementation : IIdentityPermissionEvaluatable<TestMisUse>
{
    /// <inheritdoc />
    public Dictionary<TestMisUse, IEnumerable<IIdentity>> Permissions { get; set; } = new();
}