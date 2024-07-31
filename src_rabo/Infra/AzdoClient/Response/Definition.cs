namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class Definition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Revision { get; set; }
    public TeamProjectReference Project { get; set; }
}