using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Requests;

public static class Connections
{
    public static IAzdoRequest<Response.ConnectionData> ConnectionData() =>
        new AzdoRequest<Response.ConnectionData>("_apis/connectionData",
            new Dictionary<string, object>
            {
                {"api-version", "5.0-preview"},
            });
}