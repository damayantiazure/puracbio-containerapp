namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

public class DeploymentInformation
{
    public const string AzureDevOpsMethod = "Azure Devops";
    public string? Information { get; set; }
    public string? Method { get; set; }
}