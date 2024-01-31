using System;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class Project
{
    public string Name { get; set; }

    public string Id { get; set; }

    public string Description { get; set; }

    public Uri Url { get; set; }

    public ProjectCapabilities Capabilities { get; set; }
}