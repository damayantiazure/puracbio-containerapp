using Rabobank.Compliancy.Functions.Sm9Changes.Application;
using Rabobank.Compliancy.Infra.Sm9Client.Change;
using Rabobank.Compliancy.Infra.Sm9Client.Change.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Infrastructure;

public class ServiceManagerChangeService : IChangeService
{
    private readonly IChangeClient _changeClient;

    public ServiceManagerChangeService(IChangeClient changeClient)
    {
        _changeClient = changeClient;
    }

    public async Task CloseChangesAsync(CloseChangeDetails requestBody, IEnumerable<string> changeIds)
    {
        await Task.WhenAll(changeIds.Select(changeId => CloseChangeAsync(requestBody, changeId)));            
    }

    private async Task CloseChangeAsync(CloseChangeDetails requestBody, string changeId)
    {
        var closeChange = new CloseChangeRequestBody(changeId)
        {
            ClosureCode = requestBody.CompletionCode,
            ClosureComments = string.Join(" ", requestBody.CompletionComments)
        };

        await _changeClient.CloseChangeAsync(closeChange);
    }
}