using FluentValidation;
using Rabobank.Compliancy.Application.Requests.OpenPermissions;

namespace Rabobank.Compliancy.Application.Requests.RequestValidation.OpenPermissions;

public class OpenGitRepoPermissionsRequestValidator : AbstractRequestBaseValidator<OpenGitRepoPermissionsRequest>
{
    public OpenGitRepoPermissionsRequestValidator()
    {
        RuleFor(x => x.GitRepoId).NotEmpty();
    }
}