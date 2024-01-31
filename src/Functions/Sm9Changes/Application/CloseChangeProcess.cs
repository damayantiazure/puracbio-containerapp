using Rabobank.Compliancy.Functions.Sm9Changes.Exceptions;
using Rabobank.Compliancy.Functions.Sm9Changes.Extensions;
using Rabobank.Compliancy.Functions.Sm9Changes.Model;
using Rabobank.Compliancy.Functions.Sm9Changes.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.Sm9Client.Change.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Application;

public class CloseChangeProcess : ICloseChangeProcess
{
    private readonly TimeOutSettings _changeTimeOut;
    private readonly IChangeService _changeService;
    private readonly ISm9ChangesService _sm9ChangesService;
    private readonly Dictionary<string, IAzdoService> _azdoServiceFactory;

    public CloseChangeProcess(IChangeService changeService, ISm9ChangesService sm9ChangesService, IAzdoRestClient azdoClient, TimeOutSettings changeTimeOut)
    {
        _changeService = changeService;
        _sm9ChangesService = sm9ChangesService;
        _azdoServiceFactory = new Dictionary<string, IAzdoService>
        {
            [SM9Constants.BuildPipelineType] = new AzdoBuildService(azdoClient),
            [SM9Constants.ReleasePipelineType] = new AzdoReleaseService(azdoClient, new AzdoBuildService(azdoClient))
        };
        _changeTimeOut = changeTimeOut;
    }
    public async Task<(IEnumerable<string>, IEnumerable<string>)> CloseChangeAsync(CloseChangeRequest closeChangeRequest)
    {
        var changeIds = await GetChangeIdsAsync(closeChangeRequest);

        var changeDetails = await _sm9ChangesService.ValidateChangesAsync(
            changeIds, new[] { SM9Constants.DeploymentPhase, SM9Constants.ClosurePhase, SM9Constants.AbandonedPhase },
            _changeTimeOut.TimeOutValue);

        var validChangeIds = GetValidChangeIds(changeDetails);

        var alreadyClosed = changeDetails.Where(AlreadyClosed).Select(x => x.ChangeId);

        await CloseChangesAsync(closeChangeRequest.CloseChangeDetails, validChangeIds);

        var invalidChanges = changeDetails.Where(c => !c.HasCorrectPhase);

        if (invalidChanges.Any())
        {
            throw new ChangePhaseValidationException(ErrorMessages.InvalidChangePhase(
                invalidChanges, closeChangeRequest.PipelineType, true));
        }

        return (validChangeIds, alreadyClosed);
    }

    private static bool AlreadyClosed(ChangeInformation changeInfo)
    {
        return changeInfo.Phase.Equals(SM9Constants.ClosurePhase, StringComparison.InvariantCultureIgnoreCase) ||
               changeInfo.Phase.Equals(SM9Constants.AbandonedPhase, StringComparison.InvariantCultureIgnoreCase);
    }

    private static IEnumerable<string> GetValidChangeIds(IEnumerable<ChangeInformation> changeDetails)
    {
        return changeDetails
            .Where(x => x.HasCorrectPhase && !AlreadyClosed(x))
            .Select(x => x.ChangeId);
    }

    private IAzdoService GetAzdoService(string definitionType)
    {
        return _azdoServiceFactory[definitionType];
    }

    private async Task<IEnumerable<string>> GetChangeIdsAsync(CloseChangeRequest closeChangeRequest)
    {
        var azdoService = GetAzdoService(closeChangeRequest.PipelineType);

        var changeIds = closeChangeRequest.CloseChangeDetails.ChangeId.IsValidChangeId()
            ? new[] { closeChangeRequest.CloseChangeDetails.ChangeId }
            : await azdoService.GetChangeIdsFromTagsAsync(
                closeChangeRequest.Organization, closeChangeRequest.ProjectId.ToString(),
                closeChangeRequest.RunId.ToString(), SM9Constants.ChangeIdWithUrlHashRegex);

        if (changeIds == null || !changeIds.Any())
        {
            throw new ChangeIdNotFoundException(ErrorMessages.ChangeIdNotFound(closeChangeRequest.PipelineType, true));
        }

        return changeIds;
    }

    private async Task CloseChangesAsync(CloseChangeDetails input, IEnumerable<string> changeIds)
    {
        await _changeService.CloseChangesAsync(input, changeIds);
    }
}