using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl.Http;

namespace Rabobank.Compliancy.Infra.AzdoClient;

public interface IEnumerableRequest<TResponse>
{
    IAzdoRequest<TResponse> Request { get; }
    Task<IEnumerable<TResponse>> EnumerateAsync(IFlurlRequest request);
}