#nullable enable

namespace Rabobank.Compliancy.Application.Security;
public interface ISecurityContext
{
    public Task ResolveUserFromToken(string token, string organization, CancellationToken cancellationToken);
    public Guid UserId { get; }
}
