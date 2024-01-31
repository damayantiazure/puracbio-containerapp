#nullable enable

using Rabobank.Compliancy.Domain.Compliancy.Authorizations;
using Rabobank.Compliancy.Domain.Compliancy.Evaluatables;
using Rabobank.Compliancy.Domain.Compliancy.Evaluatables.MisUsableEvaluatableTypes;

namespace Rabobank.Compliancy.Domain.Compliancy;

public class Project : IIdentityPermissionEvaluatable<ProjectMisUse>
{
    /// <summary>
    /// Unique Identifier of a Project in a given Organization Scope
    /// </summary>
    public Guid Id { get; set; }

    public string? Name { get; set; }

    /// <summary>
    /// The name of the unique Organization a Project is scoped to
    /// </summary>
    public string? Organization { get; set; }

    /// <inheritdoc />>
    public Dictionary<ProjectMisUse, IEnumerable<IIdentity>> Permissions { get; set; } = new();
}