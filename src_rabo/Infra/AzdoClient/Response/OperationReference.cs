namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class OperationReference
{
    public string Id { get; set; }

    public OperationStatus Status { get; set; }

    public string Url { get; set; }
}