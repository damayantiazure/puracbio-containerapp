#nullable enable

using Rabobank.Compliancy.Application.Security;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Infrastructure.Security;

internal class SecurityContext : ISecurityContext
{
    private readonly Application.Services.IAuthorizationService _authorizationService;

    public SecurityContext(Application.Services.IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public Guid UserId { get; private set; }

    public async Task ResolveUserFromToken(string token, string organization, CancellationToken cancellationToken = default)
    {
        var authHeader = AuthenticationHeaderValue.Parse(token);
        var user = await _authorizationService.GetCurrentUserAsync(organization, authHeader, cancellationToken);
        UserId = user.Id;
    }
}
