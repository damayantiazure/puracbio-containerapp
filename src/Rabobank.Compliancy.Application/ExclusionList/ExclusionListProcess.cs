#nullable enable
using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Exclusions;
using Rabobank.Compliancy.Domain.Exceptions;

namespace Rabobank.Compliancy.Application.ExclusionList;

/// <inheritdoc/>
public class ExclusionListProcess : IExclusionListProcess
{
    private readonly IExclusionService _exclusionService;
    private const string CreateExclusionResultMessage = "Exclusion has been registered. Please make sure you have someone else register an exclusion as well in order to finalize the exclusion!";
    private const string UpdatedExclusionResultMessage = "Exclusion request succesfully approved.";
    private static bool _exclusionCreated;

    public ExclusionListProcess(IExclusionService exclusionService)
    {
        _exclusionService = exclusionService;
    }

    /// <inheritdoc/>
    public async Task<string> CreateOrUpdateExclusionListAsync(ExclusionListRequest exclusionListRequest, User user, CancellationToken cancellationToken = default)
    {
        var exclusion = await _exclusionService.GetExclusionAsync(exclusionListRequest.Organization, exclusionListRequest.ProjectId,
            exclusionListRequest.PipelineId, exclusionListRequest.PipelineType, cancellationToken);

        // create a new exclusion record as requester
        exclusion = exclusion == null || exclusion.IsApproved || exclusion.IsExpired
            ? CreateExclusionRecord(exclusionListRequest, user)
            : UpdateExclusionRecord(exclusion, exclusionListRequest, user);

        await _exclusionService.CreateOrUpdateExclusionAsync(exclusion, cancellationToken);

        return _exclusionCreated ? CreateExclusionResultMessage : UpdatedExclusionResultMessage;
    }

    private static Exclusion CreateExclusionRecord(ExclusionListRequest exclusionListRequest, User user)
    {
        _exclusionCreated = true;
        return new(exclusionListRequest.Organization, exclusionListRequest.ProjectId,
               exclusionListRequest.PipelineId.ToString(), exclusionListRequest.PipelineType)
        {
            ExclusionReasonRequester = exclusionListRequest.Reason,
            Requester = user.MailAddress
        };
    }

    private static Exclusion UpdateExclusionRecord(Exclusion exclusion, ExclusionListRequest exclusionListRequest, User user)
    {
        _exclusionCreated = false;
        // approve the exclusion
        if (user.MailAddress == exclusion.Requester)
        {
            throw new InvalidExclusionRequesterException();
        }

        if (exclusion.Approver != null)
        {
            throw new ExclusionApproverAlreadyExistsException();
        }

        exclusion.ExclusionReasonApprover = exclusionListRequest.Reason;
        exclusion.Approver = user.MailAddress;

        return exclusion;
    }
}