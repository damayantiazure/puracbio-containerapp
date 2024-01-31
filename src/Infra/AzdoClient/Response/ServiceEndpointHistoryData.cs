namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class ServiceEndpointHistoryData
{
    public int Id { get; set; }
    public ReleaseDefinition Definition { get; set; }
    public Owner Owner { get; set; }
    public string PlanType { get; set; }
    public string StartTime { get; set; }
}