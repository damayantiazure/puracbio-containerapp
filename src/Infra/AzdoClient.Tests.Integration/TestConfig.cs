using Microsoft.Extensions.Configuration;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests.Integration;

public class TestConfig
{
    public TestConfig()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false)
            .AddJsonFile("appsettings.user.json", true)
            .AddEnvironmentVariables()
            .Build();

        Token = configuration["token"];
        ProjectName = configuration["projectName"];
        ProjectId = configuration["projectId"];
        Organization = configuration["organization"];
        ExpectedAgentPoolName = configuration["expectedAgentPoolName"] ?? "Default";
        ServiceEndpointId = configuration["serviceEndpointId"] ?? "975b3603-9939-4f22-a5a9-baebb39b5dad";
        ReleaseDefinitionId = configuration["releaseDefinitionId"] ?? "1";
        ReleaseDefinitionName = configuration["releaseDefinitionName"] ?? "AZDO-COMPLIANCY-FUNCTION (Green)";
        ReleasePipelineRevision = configuration["releasePipelineRevision"] ?? "1";
        BuildDefinitionIdYaml = configuration["buildDefinitionId"] ?? "507";
        BuildPipelineRevision = configuration["buildPipelineRevision"] ?? "6";
        BuildDefinitionIdClassic = configuration["buildDefinitionIdClassic"] ?? "505";
        BuildId = configuration["buildId"] ?? "320992";
        RepositoryId = configuration["repositoryId"] ?? "c819561e-2958-415a-b7e9-c9ad830a61c7";
        GitItemFilePath = configuration["gitItemFilePath"] ?? "/pipelines/compliant-yaml-pipeline.yml";
        CommitId = configuration["commitId"] ?? "75887e937e5b952787d8a6eb594cd1f6155fbc48";
        StageId = configuration["stageId"] ?? "9039092a-9198-5115-026a-7747c2953b49";
        PolicyId = configuration["policyId"] ?? "1642";
        ReleaseId = configuration["releaseId"] ?? "1";
        TaskGroupId = configuration["taskGroupID"] ?? "98247d2b-14b3-4c99-a917-2816997f4d62";
        Branch = "refs/heads/master";
        PipelineId = "634";

        if (int.TryParse(configuration["AgentPoolId"], out int poolId))
        {
            AgentPoolId = poolId;
        }
    }

    public string Token { get; }
    public string Branch { get; }
    public string PipelineId { get; }
    public string ProjectName { get; }
    public string ProjectId { get; }
    public string Organization { get; }
    public string ExpectedAgentPoolName { get; }
    public string ServiceEndpointId { get; }
    public string ReleaseDefinitionId { get; }
    public string ReleaseDefinitionName { get; }
    public string ReleasePipelineRevision { get; }
    public int AgentPoolId { get; } = 1;
    public string BuildId { get; }
    public string BuildDefinitionIdYaml { get; }
    public string BuildPipelineRevision { get; }
    public string BuildDefinitionIdClassic { get; }
    public string RepositoryId { get; }
    public string GitItemFilePath { get; }
    public string CommitId { get; }
    public string StageId { get; }
    public string PolicyId { get; }
    public string ReleaseId { get; }
    public string TaskGroupId { get; }
    public string QueryPipelineInfoProjectName { get; }
    public string QueryPipelineInfoProjectId { get; }
    public string QueryPipelineInfoPipelineId { get; }
}