
using System;
using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient;

public class ExtmgmtRequest<TResponse> : AzdoRequest<TResponse>
    where TResponse: new()
{
    public ExtmgmtRequest(string resource) : base(resource)
    {
    }

    public ExtmgmtRequest(string resource, IDictionary<string, object> queryParams) : base(resource, queryParams)
    {
    }

    public override Uri BaseUri(string organization) => new Uri($"https://extmgmt.dev.azure.com/{organization}/");
}