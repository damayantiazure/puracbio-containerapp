#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rabobank.Compliancy.Infra.AzdoClient.Converters;

namespace Rabobank.Compliancy.Infra.AzdoClient;

public class AzdoRestClient : IAzdoRestClient
{
    private const int _httpTimeoutInSeconds = 30;
    private readonly string? _organization;
    private readonly Random _random = new();
    private readonly string[] _tokens;

    public AzdoRestClient(string organization, string commaSeparatedAccessTokens) : this(commaSeparatedAccessTokens) =>
        _organization = organization ?? throw new ArgumentNullException(nameof(organization));

    public AzdoRestClient(string commaSeparatedAccessTokens)
    {
        _organization = null;
        if (string.IsNullOrEmpty(commaSeparatedAccessTokens))
        {
            throw new ArgumentNullException(nameof(commaSeparatedAccessTokens));
        }

        _tokens = commaSeparatedAccessTokens.Split(',');
        FlurlHttp.Configure(settings =>
        {
            var jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = { new PolicyConverter() }
            };

            settings.JsonSerializer = new NewtonsoftJsonSerializer(jsonSettings);
            settings.HttpClientFactory = new HttpClientFactory();
            settings.Timeout = TimeSpan.FromSeconds(_httpTimeoutInSeconds);
        });
    }

    public string? GetOrganization() => _organization;

    public Task<TResponse> GetAsync<TResponse>(IAzdoRequest<TResponse> request, string? organization = null)
        where TResponse : new()
    {
        var specifiedOrganization =
            organization ?? _organization ?? throw new ArgumentNullException(nameof(organization));

        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        return CheckRetry(() => BuildRequest(request, specifiedOrganization).GetJsonAsync<TResponse>(), true);
    }

    public Task<IEnumerable<TResponse>> GetAsync<TResponse>(IEnumerableRequest<TResponse> request,
        string? organization = null)
    {
        var specifiedOrganization =
            organization ?? _organization ?? throw new ArgumentNullException(nameof(organization));

        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var url = new Url(request.Request.BaseUri(specifiedOrganization))
            .AppendPathSegment(request.Request.Resource)
            .SetQueryParams(request.Request.QueryParams)
            .WithBasicAuth(string.Empty, GetToken());

        if (request.Request.TimeoutInSeconds != null)
        {
            url.WithTimeout(request.Request.TimeoutInSeconds.Value);
        }

        return request.EnumerateAsync(url);
    }

    public Task<TResponse> GetWithTokenAsync<TResponse>(IAzdoRequest<TResponse> request, string token,
        string? organization = null) where TResponse : new()
    {
        var specifiedOrganization =
            organization ?? _organization ?? throw new ArgumentNullException(nameof(organization));

        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        return CheckRetry(() => BuildRequest(request, specifiedOrganization, token).GetJsonAsync<TResponse>(), true);
    }

    public Task<string> GetAsStringAsync(IAzdoRequest request, string? organization = null)
    {
        var specifiedOrganization =
            organization ?? _organization ?? throw new ArgumentNullException(nameof(organization));

        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        return CheckRetry(() => BuildRequest(request, specifiedOrganization).GetStringAsync(), true);
    }

    public Task<TResponse> PostAsync<TInput, TResponse>(IAzdoRequest<TInput, TResponse> request, TInput body,
        string? organization = null, bool retry = false) where TResponse : new()
    {
        var specifiedOrganization =
            organization ?? _organization ?? throw new ArgumentNullException(nameof(organization));

        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        return CheckRetry(() => PostInternalAsync(request, body, specifiedOrganization), retry);
    }

    public Task<TResponse> PostWithCustomTokenAsync<TInput, TResponse>(IAzdoRequest<TInput, TResponse> request,
        TInput body, string token, string? organization = null, bool retry = false) where TResponse : new()
    {
        var specifiedOrganization =
            organization ?? _organization ?? throw new ArgumentNullException(nameof(organization));

        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        return CheckRetry(() => PostInternalAsync(request, body, specifiedOrganization, token), retry);
    }

    public Task<TResponse> PostStringAsHttpContentAsync<TInput, TResponse>(IAzdoRequest<TInput, TResponse> request,
        string body, string token, string? organization = null, bool retry = false) where TResponse : new()
    {
        var specifiedOrganization =
            organization ?? _organization ?? throw new ArgumentNullException(nameof(organization));

        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        return CheckRetry(() => PostStringAsHttpContentRetryableAsync(request, body, token, specifiedOrganization),
            retry);
    }

    public Task<TResponse> PatchAsync<TInput, TResponse>(IAzdoRequest<TInput, TResponse> request, TInput body,
        string? organization = null) where TResponse : new()
    {
        var specifiedOrganization =
            organization ?? _organization ?? throw new ArgumentNullException(nameof(organization));

        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        return PatchInternalAsync(request, body, specifiedOrganization);
    }

    public Task<TResponse> PutAsync<TInput, TResponse>(IAzdoRequest<TInput, TResponse> request, TInput body,
        string? organization = null) where TResponse : new()
    {
        var specifiedOrganization =
            organization ?? _organization ?? throw new ArgumentNullException(nameof(organization));

        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        return PutInternalAsync(request, body, specifiedOrganization);
    }

    public Task DeleteAsync(IAzdoRequest request, string? organization = null)
    {
        var specifiedOrganization =
            organization ?? _organization ?? throw new ArgumentNullException(nameof(organization));

        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        return DeleteInternalAsync(request, specifiedOrganization);
    }

    private IFlurlRequest BuildRequest(IAzdoRequest request, string? organization, string? token = null) =>
        new Url(request.BaseUri(organization))
            .AllowHttpStatus(HttpStatusCode.NotFound)
            .AppendPathSegment(request.Resource)
            .SetQueryParams(request.QueryParams)
            .WithBasicAuth(string.Empty, token ?? GetToken());

    private Task<TResponse> PostInternalAsync<TInput, TResponse>(IAzdoRequest<TInput, TResponse> request, TInput body,
        string organization, string? token = null) where TResponse : new()
    {
        var url = new Url(request.BaseUri(organization))
            .AppendPathSegment(request.Resource)
            .SetQueryParams(request.QueryParams)
            .WithBasicAuth(string.Empty, token ?? GetToken());

        if (request.TimeoutInSeconds != null)
        {
            url.WithTimeout(request.TimeoutInSeconds.Value);
        }

        return url
            .WithHeaders(request.Headers)
            .PostJsonAsync(body)
            .ReceiveJson<TResponse>();
    }

    private static async Task<TResponse> PostStringAsHttpContentRetryableAsync<TInput, TResponse>(
        IAzdoRequest<TInput, TResponse> request, string body, string token, string organization) where TResponse : new()
    {
        var url = new Url(request.BaseUri(organization))
            .AppendPathSegment(request.Resource)
            .SetQueryParams(request.QueryParams)
            .WithBasicAuth(string.Empty, token);

        if (request.TimeoutInSeconds != null)
        {
            url.WithTimeout(request.TimeoutInSeconds.Value);
        }

        var messageBytes = new ASCIIEncoding().GetBytes(body);
        HttpContent httpContent = new StreamContent(new MemoryStream(messageBytes));

        return await url
            .WithHeaders(request.Headers)
            .PostAsync(httpContent)
            .ReceiveJson<TResponse>();
    }

    private async Task<TResponse> PatchInternalAsync<TInput, TResponse>(IAzdoRequest<TInput, TResponse> request,
        TInput body, string organization) where TResponse : new() =>
        await new Url(request.BaseUri(organization))
            .AppendPathSegment(request.Resource)
            .WithBasicAuth(string.Empty, GetToken())
            .SetQueryParams(request.QueryParams)
            .WithHeaders(request.Headers)
            .PatchJsonAsync(body)
            .ReceiveJson<TResponse>()
            .ConfigureAwait(false);

    private async Task<TResponse> PutInternalAsync<TInput, TResponse>(IAzdoRequest<TInput, TResponse> request,
        TInput body, string organization) where TResponse : new()
    {
        var retryPolicy = AzdoHttpPolicies.GetRetryPolicyAsync();

        return await retryPolicy.ExecuteAsync(() => new Url(request.BaseUri(organization))
            .AppendPathSegment(request.Resource)
            .WithBasicAuth(string.Empty, GetToken())
            .SetQueryParams(request.QueryParams)
            .PutJsonAsync(body)
            .ReceiveJson<TResponse>());
    }

    private async Task DeleteInternalAsync(IAzdoRequest request, string organization)
    {
        // As a result of the explicit disposing of this HttpRequestMessage the CheckRetry method cannot be used.
        var retryPolicy = AzdoHttpPolicies.GetRetryPolicyAsync();

        (await retryPolicy.ExecuteAsync(() => new Url(request.BaseUri(organization))
            .AppendPathSegment(request.Resource)
            .WithBasicAuth(string.Empty, GetToken())
            .SetQueryParams(request.QueryParams)
            .DeleteAsync())).Dispose();
    }

    private string GetToken() => _tokens[_random.Next(_tokens.Length)];

    private static async Task<T> CheckRetry<T>(Func<Task<T>> executionDelegate, bool retry)
    {
        if (!retry)
        {
            return await executionDelegate();
        }

        var retryPolicy = AzdoHttpPolicies.GetRetryPolicyAsync();

        return await retryPolicy.ExecuteAsync(executionDelegate);
    }
}