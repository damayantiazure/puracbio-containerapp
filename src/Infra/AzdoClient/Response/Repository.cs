using System;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class Repository
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Id { get; set; }
    public string DefaultBranch { get; set; }
    public Project Project { get; set; }
    public Uri Url { get; set; }
    public Uri WebUrl { get; set; }
}