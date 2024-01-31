using Rabobank.Compliancy.Clients.AzureDevopsClient.IntegrationTests.ProjectInit.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.IntegrationTests.ProjectInit;

internal class CreateProjectInitRequest : HttpPostRequest<IProjectInitHttpClientCallHandler, object, object>
{
    private readonly string _organization;
    private readonly string _projectName;
    private readonly string _userEmailAddress;
    private readonly string _projectInitCode;
    private const string ProcessTemplate = "scrum";

    protected override string? Url => $"api/initialize/{_organization}/{_projectName}/{ProcessTemplate}/{_userEmailAddress}";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "code", _projectInitCode }
    };

    public CreateProjectInitRequest(string organization, string projectName, string userEmailAddress, string projectInitCode, IProjectInitHttpClientCallHandler httpClientCallHandler)
        : base(new object { }, httpClientCallHandler)
    {
        _organization = organization;
        _projectName = projectName;
        _userEmailAddress = userEmailAddress;
        _projectInitCode = projectInitCode;
    }
}