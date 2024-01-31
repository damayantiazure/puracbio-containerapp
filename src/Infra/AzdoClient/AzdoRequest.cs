using System;
using System.Collections.Generic;
using Flurl;
using Rabobank.Compliancy.Infra.AzdoClient.Enumerators;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;

namespace Rabobank.Compliancy.Infra.AzdoClient;

public class AzdoRequest<TInput, TResponse> : IAzdoRequest<TInput, TResponse>
{
    public string Resource { get; }
    public IDictionary<string, object> QueryParams { get; }

    public IDictionary<string, object> Headers { get; }

    public int? TimeoutInSeconds { get; set; }

    public AzdoRequest() { }

    public AzdoRequest(string resource) : this(resource, new Dictionary<string, object>())
    {
    }

    public AzdoRequest(string resource, IDictionary<string, object> queryParams) : this(resource, queryParams, new Dictionary<string, object>())
    {
    }

    public AzdoRequest(string resource, IDictionary<string, object> queryParams, IDictionary<string, object> headers)
    {
        Resource = resource;
        QueryParams = queryParams;
        Headers = headers;
    }

    public AzdoRequest(string resource, int timeoutSeconds)
    {
        Resource = resource;
        TimeoutInSeconds = timeoutSeconds;
    }

    public AzdoRequest(string resource, IDictionary<string, object> queryParams, int timeoutSeconds)
    {
        Resource = resource;
        QueryParams = queryParams;
        TimeoutInSeconds = timeoutSeconds;
    }

    public virtual Uri BaseUri(string organization) => new Uri($"https://dev.azure.com/{organization}/");

    public Url Url(string organization) => new Url(BaseUri(organization))
        .AppendPathSegment(Resource)
        .SetQueryParams(QueryParams);

    public Url Url() => Url(Organization.DefaultOrganization);
}

public class AzdoRequest<TResponse> : AzdoRequest<TResponse, TResponse>, IAzdoRequest<TResponse> where TResponse : new()
{
    public AzdoRequest() { }

    public AzdoRequest(string resource) : base(resource)
    {
    }

    public AzdoRequest(string resource, IDictionary<string, object> queryParams) : base(resource, queryParams)
    {
    }

    public AzdoRequest(string resource, int timeoutSeconds) : base(resource, timeoutSeconds)
    {
    }

    public AzdoRequest(string resource, IDictionary<string, object> queryParams, int timeoutSeconds) : base(resource, queryParams, timeoutSeconds)
    {
    }

    public IEnumerableRequest<TResponse> AsEnumerable() =>
        new EnumerableRequest<TResponse, MultipleEnumerator<TResponse>>(this);
}

public class AzdoRequest : IAzdoRequest
{
    public string Resource { get; }
    public IDictionary<string, object> QueryParams { get; }

    public IDictionary<string, object> Headers { get; }

    public int? TimeoutInSeconds { get; set; }

    public AzdoRequest() { }

    public AzdoRequest(string resource) : this(resource, new Dictionary<string, object>())
    {
    }

    public AzdoRequest(string resource, IDictionary<string, object> queryParams) : this(resource, queryParams, new Dictionary<string, object>())
    {
    }

    public AzdoRequest(string resource, IDictionary<string, object> queryParams, IDictionary<string, object> headers)
    {
        Resource = resource;
        QueryParams = queryParams;
        Headers = headers;
    }

    public AzdoRequest(string resource, int timeoutSeconds)
    {
        Resource = resource;
        TimeoutInSeconds = timeoutSeconds;
    }

    public AzdoRequest(string resource, IDictionary<string, object> queryParams, int timeoutSeconds)
    {
        Resource = resource;
        QueryParams = queryParams;
        TimeoutInSeconds = timeoutSeconds;
    }

    public Uri BaseUri(string organization) => new Uri($"https://dev.azure.com/{organization}/");

    public Url Url(string organization) => new Url(BaseUri(organization))
        .AppendPathSegment(Resource)
        .SetQueryParams(QueryParams);

    public Url Url() => Url(Organization.DefaultOrganization);
}