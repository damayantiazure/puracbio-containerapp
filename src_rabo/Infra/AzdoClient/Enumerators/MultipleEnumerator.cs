using Flurl.Http;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Infra.AzdoClient.Enumerators;

internal class MultipleEnumerator<TResponse> : IAzdoRequestEnumerator<TResponse> where TResponse : new()
{
    public async Task<IEnumerable<TResponse>> EnumerateAsync(IFlurlRequest request)
    {
        var more = true;
        var list = new List<TResponse>();
        var retryPolicy = AzdoHttpPolicies.GetRetryPolicyAsync();

        while (more)
        {
            var data = await retryPolicy.ExecuteAsync(async () =>
            {
                // Need headers & result so capture task first: https://stackoverflow.com/a/53514668/129269
                var task = request.GetAsync();

                var response = await task;
                more = response.Headers.TryGetValues("x-ms-continuationtoken", out var tokens);

                request.SetQueryParam("continuationToken", tokens?.First());

                return await task.ReceiveJson<Multiple<TResponse>>();
            });

            list.AddRange(data?.Value ?? Enumerable.Empty<TResponse>());
        }

        return list;
    }
}