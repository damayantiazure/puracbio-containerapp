using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl.Http;

namespace Rabobank.Compliancy.Infra.AzdoClient.Enumerators;

public interface IAzdoRequestEnumerator<TResponse> where TResponse : new()
{
    Task<IEnumerable<TResponse>> EnumerateAsync(IFlurlRequest request);
}