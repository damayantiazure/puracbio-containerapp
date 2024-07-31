using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Linq;
using System.Net.Http;

namespace Rabobank.Compliancy.Functions.ValidateGates.Model;

public class ValidateApproversAzdoData
{
    public ValidateApproversAzdoData(HttpRequestMessage request, string organization, string smoketestOrganization,
        string projectId, string runId, string stageId, Release release)
    {
        HubName = request.Headers.GetValues("HubName").FirstOrDefault();
        PlanId = request.Headers.GetValues("PlanId").FirstOrDefault();
        JobId = request.Headers.GetValues("JobId").FirstOrDefault();
        TaskInstanceId = request.Headers.GetValues("TaskInstanceId").FirstOrDefault();
        Token = request.Headers.GetValues("AuthToken").FirstOrDefault();
        Organization = organization;
        ProjectIdCallback = request.Headers.GetValues("ProjectId").FirstOrDefault();
        SmoketestOrganization = smoketestOrganization;
        ProjectId = projectId;
        RunId = runId;
        StageId = stageId;
        Release = release;
    }

    public ValidateApproversAzdoData()
    {

    }

    public string HubName { get; set; }

    public string PlanId { get; set; }

    public string JobId { get; set; }

    public string TaskInstanceId { get; set; }

    public string Organization { get; set; }

    public string ProjectId { get; set; }

    /// <summary>
    /// This projectId is used to construct the url for the callback to Azure DevOps
    /// </summary>
    public string ProjectIdCallback { get; set; }

    public string RunId { get; set; }

    public string StageId { get; set; }

    public Release Release { get; set; }

    public string Token { get; set; }

    public string SmoketestOrganization { get; set; }

    public bool IsValid =>
        !(string.IsNullOrEmpty(PlanId) ||
          string.IsNullOrEmpty(JobId) ||
          string.IsNullOrEmpty(HubName) ||
          string.IsNullOrEmpty(ProjectId) ||
          string.IsNullOrEmpty(Organization) ||
          string.IsNullOrEmpty(TaskInstanceId) ||
          string.IsNullOrEmpty(ProjectIdCallback) ||
          string.IsNullOrEmpty(Token));
}