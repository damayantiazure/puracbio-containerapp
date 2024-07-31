using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl.Http;
using Rabobank.Compliancy.Infra.AzdoClient.Enumerators;

namespace Rabobank.Compliancy.Infra.AzdoClient;

public class EnumerableRequest<TResponse, TEnumerator> : IEnumerableRequest<TResponse>
    where TEnumerator : IAzdoRequestEnumerator<TResponse>, new() where TResponse : new()
{
    public IAzdoRequest<TResponse> Request { get; }

    public EnumerableRequest(IAzdoRequest<TResponse> request)
    {
        Request = request;
    }

    public Task<IEnumerable<TResponse>> EnumerateAsync(IFlurlRequest request) =>
        new TEnumerator().EnumerateAsync(request);
}