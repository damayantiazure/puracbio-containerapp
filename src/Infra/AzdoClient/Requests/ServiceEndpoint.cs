
using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Requests;

public static class ServiceEndpoint
{
    public static IEnumerableRequest<Response.ServiceEndpointHistory> History(string project, string id) =>
        new AzdoRequest<Response.ServiceEndpointHistory>($"{project}/_apis/serviceendpoint/{id}/executionhistory").AsEnumerable();

    public static IAzdoRequest<Response.Multiple<Response.ServiceEndpointHistory>> History(string project, string id, int top) =>
        new AzdoRequest<Response.Multiple<Response.ServiceEndpointHistory>>(
            $"{project}/_apis/serviceendpoint/{id}/executionhistory", new Dictionary<string, object>
            {
                {"top", top},
                {"api-version", "6.1-preview"}
            });

    public static IEnumerableRequest<Response.ServiceEndpoint> Endpoints(string project) =>
        new AzdoRequest<Response.ServiceEndpoint>($"{project}/_apis/serviceendpoint/endpoints/").AsEnumerable();

    public static IAzdoRequest<Response.ServiceEndpoint> Endpoint(string project, System.Guid? id) =>
        new AzdoRequest<Response.ServiceEndpoint>($"{project}/_apis/serviceendpoint/endpoints/{id}", new Dictionary<string, object> { ["api-version"] = "5.1-preview.2" });

    public static IAzdoRequest<Response.ServiceEndpoint> Endpoint(string project) =>
        Endpoint(project, null);
}