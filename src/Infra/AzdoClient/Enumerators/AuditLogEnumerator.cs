using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl.Http;
using Rabobank.Compliancy.Infra.AzdoClient.Response;

namespace Rabobank.Compliancy.Infra.AzdoClient.Enumerators;

internal class AuditLogEnumerator : IAzdoRequestEnumerator<AuditLogEntry>
{
    public async Task<IEnumerable<AuditLogEntry>> EnumerateAsync(IFlurlRequest request)
    {
        var more = true;
        var list = new List<AuditLogEntry>();

        while (more)
        {
            var result = await request.GetJsonAsync<AuditLogEntries>();
            list.AddRange(result.DecoratedAuditLogEntries);

            request.SetQueryParam("continuationToken", result.ContinuationToken);
            more = result.ContinuationToken != null;
        }

        return list;
    }
}