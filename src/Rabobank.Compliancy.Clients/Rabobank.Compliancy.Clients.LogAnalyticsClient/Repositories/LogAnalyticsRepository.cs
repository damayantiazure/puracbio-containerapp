using Rabobank.Compliancy.Clients.HttpClientExtensions;
using Rabobank.Compliancy.Clients.LogAnalyticsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.LogAnalyticsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Clients.LogAnalyticsClient.Requests.Authentication;
using Rabobank.Compliancy.Clients.LogAnalyticsClient.Requests.Authentication.Models;
using Rabobank.Compliancy.Clients.LogAnalyticsClient.Requests.Workspace;
using Rabobank.Compliancy.Clients.LogAnalyticsClient.Requests.Workspace.Models;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.LogAnalyticsClient.Repositories;

/// <inheritdoc/>
public class LogAnalyticsRepository : ILogAnalyticsRepository
{
    private readonly IHttpClientCallDistributor<IMicrosoftOnlineHandler> _authenticationCallDistributor;
    private readonly IHttpClientCallDistributor<ILogAnalyticsCallHandler> _logAnalyticsCallDistributor;
    private readonly ILogAnalyticsConfiguration _logAnalyticsConfiguration;
    private Authentication? _authentication;

    public LogAnalyticsRepository(IHttpClientCallDistributor<IMicrosoftOnlineHandler> authenticationCallDistributor,
        IHttpClientCallDistributor<ILogAnalyticsCallHandler> logAnalyticsCallDistributor, ILogAnalyticsConfiguration logAnalyticsConfiguration)
    {
        _authenticationCallDistributor = authenticationCallDistributor;
        _logAnalyticsCallDistributor = logAnalyticsCallDistributor;
        _logAnalyticsConfiguration = logAnalyticsConfiguration;
    }

    /// <inheritdoc/>
    public async Task<Authentication?> GetAuthenticationAsync(CancellationToken cancellationToken = default) =>
        _authentication = _authentication == null || _authentication.IsExpired
            ? await new GetAuthenticationRequest(_logAnalyticsConfiguration.TenantId, _logAnalyticsConfiguration.ContentParameters, _authenticationCallDistributor)
                .ExecuteAsync(cancellationToken: cancellationToken)
            : _authentication;

    /// <inheritdoc/>
    public async Task<WorkspaceResponse?> QueryWorkspaceAsync(string kustoQuery, Authentication authentication, CancellationToken cancellationToken = default) =>
        await new GetWorkspaceQueryRequest(_logAnalyticsConfiguration.WorkspaceId, kustoQuery, _logAnalyticsCallDistributor)
            .ExecuteAsync(new AuthenticationHeaderValue(authentication.TokenType, authentication.AccesToken), cancellationToken);
}