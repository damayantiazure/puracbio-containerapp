#nullable enable

using Rabobank.Compliancy.Application.Requests.OpenPermissions;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Application.Interfaces.OpenPermissions;

/// <summary>
/// A process definition for opening the git repository permissions for a specific project.
/// </summary>
public interface IOpenGitRepoPermissionsProcess : IOpenProtectedResourcePermissionsProcess<OpenGitRepoPermissionsRequest, GitRepo>
{
}