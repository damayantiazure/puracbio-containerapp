#nullable enable

using Microsoft.Azure.WebJobs;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.AuditLogging;
public class ManageHooksFunction
{
    private readonly string[] _organizations;
    private readonly IManageHooksService _manageHooksService;

    public ManageHooksFunction(
        string[] organizations,
        IManageHooksService manageHooksService)
    {
        _organizations = organizations ?? throw new ArgumentNullException(nameof(organizations));
        _manageHooksService = manageHooksService ?? throw new ArgumentNullException(nameof(manageHooksService));
    }

    [FunctionName(nameof(ManageHooksFunction))]
    public Task RunAsync(
        [TimerTrigger("0 0 6 * * *", RunOnStartup = false)] TimerInfo timerInfo) =>
            Task.WhenAll(_organizations.Select(_manageHooksService.ManageHooksOrganizationAsync));
}