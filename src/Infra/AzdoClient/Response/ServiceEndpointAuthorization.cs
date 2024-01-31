using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class ServiceEndpointAuthorization
{
    public string Scheme { get; set; }

    public IDictionary<string, object> Parameters { get; set; }

    public static ServiceEndpointAuthorization UserNamePassword(string userName, string password) =>
        new()
        {
            Scheme = "UsernamePassword",
            Parameters = new Dictionary<string, object>()
            {
                ["username"] = userName,
                ["password"] = password
            }
        };
}