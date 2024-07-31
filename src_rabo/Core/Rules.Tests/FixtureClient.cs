using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using Rabobank.Compliancy.Infra.AzdoClient;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Rules.Tests;

public class FixtureClient : IAzdoRestClient
{
    private readonly IFixture _fixture;

    public FixtureClient(IFixture fixture)
    {
        _fixture = fixture;
    }

    public string GetOrganization() => _fixture.Create<string>();

    public Task<TResponse> GetAsync<TResponse>(IAzdoRequest<TResponse> request, string organization = null) where TResponse : new() =>
        Task.FromResult(_fixture.Create<TResponse>());

    public Task<TResponse> GetAsync<TResponse>(Uri url) where TResponse : new() =>
        Task.FromResult(_fixture.Create<TResponse>());

    public Task<IEnumerable<TResponse>> GetAsync<TResponse>(IEnumerableRequest<TResponse> request, string organization = null) =>
        Task.FromResult(_fixture.CreateMany<TResponse>());

    public Task<IEnumerable<TResponse>> GetAsync<TResponse>(IEnumerableRequest<TResponse> request, params HttpStatusCode[] statusCodes) =>
        Task.FromResult(_fixture.CreateMany<TResponse>());

    public Task<TResponse> PostAsync<TInput, TResponse>(IAzdoRequest<TInput, TResponse> request, TInput body, string organization = null, bool retry = false) where TResponse : new() =>
        throw new NotImplementedException();
        
    public Task<TResponse> PostWithCustomTokenAsync<TInput, TResponse>(IAzdoRequest<TInput, TResponse> request, TInput body, string token, string organization = null, bool retry = false) where TResponse : new()
    {
        throw new NotImplementedException();
    }

    public Task<TResponse> PatchAsync<TInput, TResponse>(IAzdoRequest<TInput, TResponse> request, TInput body, string organization = null) where TResponse : new() =>
        throw new NotImplementedException();

    public Task<TResponse> PutAsync<TInput, TResponse>(IAzdoRequest<TInput, TResponse> request, TInput body, string organization = null) where TResponse : new() =>
        throw new NotImplementedException();

    public Task DeleteAsync(IAzdoRequest request, string organization = null) =>
        throw new NotImplementedException();

    public Task<string> GetStreamAsStringAsync(IAzdoRequest request, string organization = null)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetAsStringAsync(IAzdoRequest request, string organization = null)
    {
        throw new NotImplementedException();
    }

    public Task<TResponse> PostStringAsHttpContentAsync<TInput, TResponse>(IAzdoRequest<TInput, TResponse> request, string body, string token, string organization = null, bool retry = false) where TResponse : new()
    {
        throw new NotImplementedException();
    }

    public Task<TResponse> GetWithTokenAsync<TResponse>(IAzdoRequest<TResponse> request, string token, string organization = null) where TResponse : new()
    {
        throw new NotImplementedException();
    }
}