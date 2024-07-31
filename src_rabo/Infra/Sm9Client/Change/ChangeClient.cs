using Newtonsoft.Json;
using Rabobank.Compliancy.Infra.Sm9Client.Change.Model;

namespace Rabobank.Compliancy.Infra.Sm9Client.Change;

public class ChangeClient : IChangeClient
{
    private readonly HttpClient _httpClient;

    public ChangeClient(IHttpClientFactory httpClientFactory) =>
        _httpClient = httpClientFactory.CreateClient(nameof(ChangeClient));

    public Task<CreateChangeResponse?> CreateChangeAsync(CreateChangeRequestBody requestBody) =>
        PostAsync<CreateChangeResponse>("createChange", new CreateChangeRequest { Body = requestBody });

    public Task<CloseChangeResponse?> CloseChangeAsync(CloseChangeRequestBody requestBody) =>
        PostAsync<CloseChangeResponse>("closeChange", new CloseChangeRequest { Body = requestBody });

    public Task<UpdateChangeResponse?> UpdateChangeAsync(UpdateChangeRequestBody requestBody) =>
        PostAsync<UpdateChangeResponse>("updateChange", new UpdateChangeRequest { UpdateChange = requestBody });

    public Task<GetChangeByKeyResponse?> GetChangeByKeyAsync(GetChangeByKeyRequestBody requestBody) =>
        PostAsync<GetChangeByKeyResponse>("retrieveChangeInfoByKey", new GetChangeByKeyRequest { Body = requestBody });

    public async Task<TResponseMessage?> PostAsync<TResponseMessage>(string requestUri, object requestBody)
    {
        HttpResponseMessage? httpResponseMessage = null;

        try
        {
            var httpContent = requestBody.ToHttpContent();

            httpResponseMessage = await _httpClient.PostAsync(requestUri, httpContent);
            httpResponseMessage.EnsureSuccessStatusCode();

            return await httpResponseMessage.ToResponseObjectAsync<TResponseMessage>();
        }
        catch (HttpRequestException ex)
        {
            var responseText = httpResponseMessage == null
                ? null
                : await httpResponseMessage.Content.ReadAsStringAsync();

            throw new ChangeClientException(
                $@"An unexpected error occurred when calling the SM9 API:
                    URL = {httpResponseMessage?.RequestMessage?.RequestUri},
                    Body = {JsonConvert.SerializeObject(requestBody)},
                    Response = {responseText}", ex);
        }
    }
}