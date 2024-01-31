using Flurl;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Extensions;

public static class AzdoRestRequestExtensions
{
    public static IAzdoRequest<JObject> AsJson<TResponse>(
        this IAzdoRequest<TResponse> request)
        where TResponse : new()
    {
        return new JsonRequest(request);
    }

    private class JsonRequest : IAzdoRequest<JObject>
    {
        private readonly IAzdoRequest _request;

        public JsonRequest(IAzdoRequest request)
        {
            _request = request;
        }

        public Uri BaseUri(string organization) => _request.BaseUri(organization);

        public string Resource => _request.Resource;
        public IDictionary<string, object> QueryParams => _request.QueryParams;

        public IDictionary<string, object> Headers => _request.Headers;
            
        public Url Url(string organization) => _request.Url(organization);

        public Url Url() => _request.Url();
        public int? TimeoutInSeconds => _request.TimeoutInSeconds;
    }
}