namespace Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers;

public static class SecurityNamespaces
{
    public static readonly Guid Build = new("33344d9c-fc72-4d6f-aba5-fa317101a7e9");
    public static readonly Guid GitRepo = new("2e9eb7ed-3c0a-47d4-87c1-0ffdd275fd87");
    public static readonly Guid ReleaseManagement = new("c788c23e-1b46-4162-8f5e-d7585343b5de");
    public static readonly Guid Project = new("52d39943-cb85-4d7f-8fa8-c6baac873819");
    public static readonly Guid DistributedTask = new("101eae8c-1709-47f9-b228-0e476c35b3ba");
    public static readonly Guid Environment = new("83d4c2e6-e57d-4d6e-892b-b87222b7ad20");
}