using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.ReleaseRequests;

public class GetReleaseApprovalsRequest :
    HttpGetRequest<IVsrmHttpClientCallHandler, ResponseCollection<ReleaseApproval>>
{
    private readonly string _organization;
    private readonly Guid _projectId;
    private readonly int _releaseId;
    private readonly ApprovalStatus _status;

    public GetReleaseApprovalsRequest(string organization, Guid projectId, int releaseId, ApprovalStatus status,
        IVsrmHttpClientCallHandler httpClientCallHandler) : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
        _releaseId = releaseId;
        _status = status;
    }

    protected override string Url => $"{_organization}/{_projectId}/_apis/release/approvals";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "releaseIdsFilter", _releaseId.ToString() },
        { "statusFilter", _status.ToString().ToLower() },
        { "api-version", "7.0" }
    };
}