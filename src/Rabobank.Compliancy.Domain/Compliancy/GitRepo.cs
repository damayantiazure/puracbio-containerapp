using Rabobank.Compliancy.Domain.Compliancy.Authorizations;
using Rabobank.Compliancy.Domain.Compliancy.Evaluatables;
using Rabobank.Compliancy.Domain.Compliancy.Evaluatables.MisUsableEvaluatableTypes;

namespace Rabobank.Compliancy.Domain.Compliancy;

public class GitRepo : PipelineResource, IIdentityPermissionEvaluatable<RepositoryMisUse>, IProtectedResource
{
    /// <summary>
    /// Unique Identifier of a <see cref="GitRepo"/> in a given <see cref="Project"/> Scope
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// The direct URL to the <see cref="GitRepo"/>
    /// </summary>
    public Uri Url { get; set; }

    /// <inheritdoc />>
    public Dictionary<RepositoryMisUse, IEnumerable<IIdentity>> Permissions { get; set; } = new();
}