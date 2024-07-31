using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;

namespace Rabobank.Compliancy.Clients.HttpClientExtensions;

/// <inheritdoc/>
public class HttpClientCallHandler : IHttpClientCallHandler
{
    public string Identifier => HttpClientName;

    public long CurrentTotalWeight { get; set; }

    /// <inheritdoc/>
    public HttpClientCallHandler(IHttpClientFactory httpClientFactory, string httpClientName)
    {
        HttpClientName = httpClientName;
        ClientFactory = httpClientFactory;
    }

    protected string HttpClientName { get; }
    protected IHttpClientFactory ClientFactory { get; }

    /// <inheritdoc/>
    public async Task HandleDeleteCallAsync(Uri uri, AuthenticationHeaderValue? authenticationHeaderValue = default, CancellationToken cancellationToken = default)
    {
        using var httpClient = CreateAndCustomizeHttpClient(authenticationHeaderValue);
        var httpResponseMessage = await httpClient.DeleteAsync(uri, cancellationToken);
        httpResponseMessage.EnsureSuccessStatusCode();
    }

    /// <inheritdoc/>
    public async Task<TResponse?> HandleDeleteCallAsync<TResponse>(Uri uri, AuthenticationHeaderValue? authenticationHeaderValue = default, CancellationToken cancellationToken = default)
    {
        using var httpClient = CreateAndCustomizeHttpClient(authenticationHeaderValue);
        var httpResponseMessage = await httpClient.DeleteAsync(uri, cancellationToken);
        httpResponseMessage.EnsureSuccessStatusCode();
        return await httpResponseMessage.Content.ReadAsAsync<TResponse>(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<TResponse?> HandleGetCallAsync<TResponse>(Uri uri, AuthenticationHeaderValue? authenticationHeaderValue = default, CancellationToken cancellationToken = default)
    {
        using var httpClient = CreateAndCustomizeHttpClient(authenticationHeaderValue);
        var httpResponseMessage = await httpClient.GetAsync(uri, cancellationToken);
        httpResponseMessage.EnsureSuccessStatusCode();
        return await httpResponseMessage.Content.ReadAsAsync<TResponse>(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<TResponse?> HandlePostCallAsync<TResponse, TValue>(Uri uri, TValue value, AuthenticationHeaderValue? authenticationHeaderValue = default, CancellationToken cancellationToken = default)
    {
        using var httpClient = CreateAndCustomizeHttpClient(authenticationHeaderValue);
        var httpResponseMessage = value is HttpContent httpContent
            ? await httpClient.PostAsync(uri, httpContent, cancellationToken)
            : await httpClient.PostAsync(uri, value, GetMediaTypeFormatter(value), cancellationToken);
        httpResponseMessage.EnsureSuccessStatusCode();
        return await httpResponseMessage.Content.ReadAsAsync<TResponse>(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<TResponse?> HandlePutCallAsync<TResponse, TValue>(Uri uri, TValue value, AuthenticationHeaderValue? authenticationHeaderValue = default, CancellationToken cancellationToken = default)
    {
        using var httpClient = CreateAndCustomizeHttpClient(authenticationHeaderValue);
        var httpResponseMessage = await httpClient.PutAsync(uri, value, CreateJsonMediaTypeFormatter(), cancellationToken);
        httpResponseMessage.EnsureSuccessStatusCode();
        return await httpResponseMessage.Content.ReadAsAsync<TResponse>(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<TResponse?> HandlePatchCallAsync<TResponse, TValue>(Uri uri, TValue value, AuthenticationHeaderValue? authenticationHeaderValue = default, CancellationToken cancellationToken = default)
    {
        using var httpClient = CreateAndCustomizeHttpClient(authenticationHeaderValue);

        var valueAsJson = JsonConvert.SerializeObject(value);
        var stringContent = new StringContent(valueAsJson, Encoding.UTF8, "application/json");

        var httpResponseMessage = await httpClient.PatchAsync(uri, stringContent, cancellationToken);
        httpResponseMessage.EnsureSuccessStatusCode();
        return await httpResponseMessage.Content.ReadAsAsync<TResponse>(cancellationToken);
    }

    protected virtual HttpClient CreateAndCustomizeHttpClient(AuthenticationHeaderValue? authenticationHeaderValue)
    {
        var httpClient = ClientFactory.CreateClient(HttpClientName);
        AddCustomTokenToHttpClient(httpClient, authenticationHeaderValue);
        return httpClient;
    }

    protected static void AddCustomTokenToHttpClient(HttpClient httpClient, AuthenticationHeaderValue? authenticationHeaderValue)
    {
        if (authenticationHeaderValue == null)
        {
            return;
        }

        httpClient.DefaultRequestHeaders.Authorization = authenticationHeaderValue;
    }

    private static MediaTypeFormatter CreateJsonMediaTypeFormatter() =>
        new JsonMediaTypeFormatter
        {
            SerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }
        };

    private static MediaTypeFormatter CreateStreamMediaTypeFormatter() =>
        new BinaryMediaTypeFormatter();

    private static MediaTypeFormatter GetMediaTypeFormatter<TValue>(TValue value) =>
     value switch
     {
         byte[] => CreateStreamMediaTypeFormatter(),
         _ => CreateJsonMediaTypeFormatter(),
     };
}