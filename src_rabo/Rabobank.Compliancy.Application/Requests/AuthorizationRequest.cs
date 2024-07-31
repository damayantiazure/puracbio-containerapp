namespace Rabobank.Compliancy.Application.Requests;

public class AuthorizationRequest
{
    public string Organization { get; }
    public Guid ProjectId { get; }

    public AuthorizationRequest(Guid projectId, string organization)
    {
        Organization = organization;
        ProjectId = projectId;
    }
}