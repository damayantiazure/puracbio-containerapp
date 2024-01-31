namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class AgentQueue
{
    public int Id { get; set; }
    public string Name { get; set; }
    public AgentPool Pool { get; set; }
}