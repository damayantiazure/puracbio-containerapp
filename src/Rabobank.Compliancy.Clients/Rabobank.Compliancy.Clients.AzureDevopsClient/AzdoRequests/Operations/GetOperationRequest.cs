using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Operations;

using Microsoft.VisualStudio.Services.Operations;

/// <summary>
/// Used to get Operation by Id from the URL "{_organization}/_apis/operations/{_operationId}".
/// </summary>
public class GetOperationRequest : HttpGetRequest<IDevHttpClientCallHandler, Operation>
{
    private readonly string _organization;
    private readonly Guid _operationId;

    protected override string Url => $"{_organization}/_apis/operations/{_operationId}";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        {"api-version", "7.0"}
    };

    public GetOperationRequest(string organization, Guid id, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _operationId = id;
    }
}