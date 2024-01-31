using System;
using System.Collections.Generic;
using Rabobank.Compliancy.Infra.AzdoClient.Enumerators;

namespace Rabobank.Compliancy.Infra.AzdoClient;

public class VsrmRequest<TInput, TResponse> : AzdoRequest<TInput, TResponse>
    where TResponse: new()
{
    public VsrmRequest(string resource) : base(resource)
    {
    }
        
    public VsrmRequest(string resource, IDictionary<string, object> queryParams) : base(resource, queryParams)
    {
    }

    public override Uri BaseUri(string organization) => new Uri($"https://vsrm.dev.azure.com/{organization}/");
}

public class VsrmRequest<TResponse> : VsrmRequest<TResponse, TResponse>, IAzdoRequest<TResponse>
    where TResponse: new()
{
    public VsrmRequest(string resource) : base(resource)
    {
    }
        
    public VsrmRequest(string resource, IDictionary<string, object> queryParams) : base(resource, queryParams)
    {
    }
        
    public IEnumerableRequest<TResponse> AsEnumerable() => 
        new EnumerableRequest<TResponse, MultipleEnumerator<TResponse>>(this);
}