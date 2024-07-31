#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Infra.AzdoClient;

public interface IAzdoRestClient
{
    string? GetOrganization(); //Prettiest if this method can be removed after all functions support multiple organizations
    Task<TResponse> GetAsync<TResponse>(IAzdoRequest<TResponse> request, string? organization = null) where TResponse : new();        
    Task<IEnumerable<TResponse>> GetAsync<TResponse>(IEnumerableRequest<TResponse> request, string? organization = null);
    Task<TResponse> GetWithTokenAsync<TResponse>(IAzdoRequest<TResponse> request, string token, string? organization = null) where TResponse : new();
    Task<string> GetAsStringAsync(IAzdoRequest request, string? organization = null);
    Task<TResponse> PostAsync<TInput, TResponse>(IAzdoRequest<TInput, TResponse> request, TInput body, string? organization = null, bool retry = false) where TResponse : new();
    Task<TResponse> PostWithCustomTokenAsync<TInput, TResponse>(IAzdoRequest<TInput, TResponse> request, TInput body, string token, string? organization = null, bool retry = false) where TResponse : new();
    Task<TResponse> PostStringAsHttpContentAsync<TInput, TResponse>(IAzdoRequest<TInput, TResponse> request, string body, string token, string? organization = null, bool retry = false) where TResponse : new();
    Task<TResponse> PatchAsync<TInput, TResponse>(IAzdoRequest<TInput, TResponse> request, TInput body, string? organization = null) where TResponse : new();
    Task<TResponse> PutAsync<TInput, TResponse>(IAzdoRequest<TInput, TResponse> request, TInput body, string? organization = null) where TResponse : new();
    Task DeleteAsync(IAzdoRequest request, string? organization = null);
}