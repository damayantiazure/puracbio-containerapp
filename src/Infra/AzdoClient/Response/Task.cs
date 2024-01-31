namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class Task
{
    public string Id { get; set; }
    public string Name { get; set; }
    public Status? Status { get; set; }
}