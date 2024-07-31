using Rabobank.Compliancy.Clients.HttpClientExtensions;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;
using Rabobank.Compliancy.Clients.LogAnalyticsClient.HttpClientCallHandlers.Interfaces;

namespace Rabobank.Compliancy.Clients.LogAnalyticsClient.Requests.Authentication;

public class GetAuthenticationRequest : HttpPostRequest<IMicrosoftOnlineHandler, Models.Authentication, FormUrlEncodedContent>
{
    private readonly string _tenantId;

    protected override string? Url => $"{_tenantId}/oauth2/token";

    protected override Dictionary<string, string> QueryStringParameters => new();

    public GetAuthenticationRequest(string tenantId, IDictionary<string, string> headerContent, IHttpClientCallDistributor<IMicrosoftOnlineHandler> callDistributor)
        : base(new FormUrlEncodedContent(headerContent), callDistributor)
    {
        _tenantId = tenantId;
    }
}