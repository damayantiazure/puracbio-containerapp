using Flurl.Http;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Infra.AzdoClient.Enumerators;

public class MemberEntitlementsEnumerator<TResponse> : IAzdoRequestEnumerator<TResponse> where TResponse : new()
{
    public async Task<IEnumerable<TResponse>> EnumerateAsync(IFlurlRequest request)
    {
        var more = true;
        var list = new List<TResponse>();
        var retryPolicy = AzdoHttpPolicies.GetRetryPolicyAsync();

        while (more)
        {
            var data = await retryPolicy.ExecuteAsync(() => request.GetAsync().ReceiveJson<Entitlements<TResponse>>());

            list.AddRange(data?.Items ?? Enumerable.Empty<TResponse>());
            more = !string.IsNullOrEmpty(data?.ContinuationToken);

            if (more)
            {
                request.SetQueryParam("continuationToken", data.ContinuationToken);
            }
        }

        return list;
    }
}