namespace Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers;

public static class PermissionExtensions
{
    public static string ExtractPipelinePermissionSetToken(string pipelineId, string pipelinePath, Guid projectId)
    {
        return pipelinePath == "\\" || string.IsNullOrEmpty(pipelinePath)
            ? $"{projectId}/{pipelineId}"
            : $"{projectId}{pipelinePath.Replace("\\", "/", StringComparison.InvariantCulture)}/{pipelineId}";
    }
}