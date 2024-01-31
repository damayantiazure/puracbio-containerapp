using System;
using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient;

public class AuditRequest<T> : AzdoRequest<T> where T: new()
{
    public AuditRequest(string resource) : base(resource)
    {
    }

    public AuditRequest(string resource, Dictionary<string, object> queryParams) : base(resource, queryParams)
    {
    }

    public override Uri BaseUri(string organization) => new Uri($"https://auditservice.dev.azure.com/{organization}");
}