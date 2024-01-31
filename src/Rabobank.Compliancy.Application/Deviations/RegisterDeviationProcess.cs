#nullable enable

using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Deviations;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Application.Deviations;

public class RegisterDeviationProcess : IRegisterDeviationProcess
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ICompliancyReportService _compliancyReportService;
    private readonly IDeviationService _deviationService;
    private readonly IProjectService _projectService;

    public RegisterDeviationProcess(IAuthorizationService authorizationService, IProjectService projectService,
        IDeviationService deviationService, ICompliancyReportService compliancyReportService)
    {
        _authorizationService = authorizationService;
        _projectService = projectService;
        _deviationService = deviationService;
        _compliancyReportService = compliancyReportService;
    }

    /// <inheritdoc />
    public async Task RegisterDeviation(RegisterDeviationRequest registerDeviationRequest,
        AuthenticationHeaderValue authenticationHeaderValue, CancellationToken cancellationToken = default)
    {
        registerDeviationRequest.Validate();

        var project = await _projectService.GetProjectByIdAsync(registerDeviationRequest.Organization,
            registerDeviationRequest.ProjectId, cancellationToken);
        var user = await _authorizationService.GetCurrentUserAsync(project.Organization, authenticationHeaderValue,
            cancellationToken);
        var deviation = RequestToDeviationObject(registerDeviationRequest, project, user);

        await _deviationService.CreateOrReplaceDeviationAsync(deviation, user.Username!, cancellationToken);
        var createdDeviation = await _deviationService.GetDeviationAsync(deviation.Project, deviation.RuleName,
            deviation.ItemId, deviation.CiIdentifier, deviation.ItemProjectId, cancellationToken);
        await _compliancyReportService.AddDeviationToReportAsync(createdDeviation);

        // Send deviation insert record to the queue
        await _deviationService.SendDeviationUpdateRecord(deviation, DeviationReportLogRecordType.Insert);
    }

    private static Deviation RequestToDeviationObject(RegisterDeviationRequest registerDeviationRequest,
        Project project, User user) =>
        new(registerDeviationRequest.ItemId!, registerDeviationRequest.RuleName!,
            registerDeviationRequest.CiIdentifier!, project, registerDeviationRequest.Reason,
            registerDeviationRequest.ReasonNotApplicable, registerDeviationRequest.ReasonOther,
            registerDeviationRequest.ReasonNotApplicableOther, registerDeviationRequest.Comment!)
        {
            ItemProjectId = registerDeviationRequest.ForeignProjectId,
            UpdatedBy = user.Username
        };
}