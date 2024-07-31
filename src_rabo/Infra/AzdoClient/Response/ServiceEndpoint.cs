using System;
using Newtonsoft.Json;
using Rabobank.Compliancy.Infra.AzdoClient.Converters;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class ServiceEndpoint
{
    public string Name { get; set; }
    public Guid Id { get; set; }
    public string Type { get; set; }
    public Uri Url { get; set; }
    public IdentityRef CreatedBy { get; set; }
    public ServiceEndpointAuthorization Authorization { get; set; }
}