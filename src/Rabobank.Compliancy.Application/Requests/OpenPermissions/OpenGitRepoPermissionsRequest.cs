#nullable enable

using FluentValidation.Results;
using Rabobank.Compliancy.Application.Requests.RequestValidation.OpenPermissions;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Application.Requests.OpenPermissions;

public class OpenGitRepoPermissionsRequest : OpenPermissionsRequestBase<GitRepo>
{
    private readonly OpenGitRepoPermissionsRequestValidator _validator = new();

    public Guid GitRepoId { get; set; }

    /// <inheritdoc/>
    public override ValidationResult Validate() => _validator.Validate(this);
}