#nullable enable

using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;

public class PipelineRunInfo
{
    public string Organization { get; set; }
    public string ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? PipelineId { get; set; }
    public string? PipelineName { get; set; }
    public IList<StageReport>? Stages { get; set; }
    public string? PipelineType { get; set; }
    public string? PipelineVersion { get; set; }
    public string? RunId { get; set; }
    public string? RunUrl { get; set; }
    public string? StageId { get; set; }
    public ReleaseDefinition? ClassicReleasePipeline { get; set; }
    public BuildDefinition? BuildPipeline { get; set; }
    public string? ErrorMessage { get; set; }

    public PipelineRunInfo(string organization, string projectId, string pipelineId,
        string pipelineType)
    {
        Organization = organization;
        ProjectId = projectId;
        PipelineId = pipelineId;
        PipelineType = pipelineType;
    }

    public PipelineRunInfo(string organization, string projectId, string runId, string stageId,
        string pipelineType)
    {
        Organization = organization;
        ProjectId = projectId;
        PipelineType = pipelineType;
        RunId = runId;
        StageId = stageId;
    }

    public PipelineRunInfo(string organization, Project project, ReleaseDefinition classicReleasePipeline,
        IList<StageReport> stages, Release release, string? stageId)
    {
        Organization = organization;
        ProjectId = project.Id;
        ProjectName = project.Name;
        PipelineId = classicReleasePipeline.Id;
        PipelineName = classicReleasePipeline.Name;
        Stages = stages;
        PipelineType = ItemTypes.ClassicReleasePipeline;
        PipelineVersion = release.ReleaseDefinitionRevision;
        RunId = release.Id.ToString();
        RunUrl = release.Links?.Web.Href.AbsoluteUri;
        StageId = stageId;
        ClassicReleasePipeline = classicReleasePipeline;
    }

    public PipelineRunInfo(string organization, BuildDefinition buildPipeline, IList<StageReport>? stages,
        string? pipelineType, Build build, string? stageId, string? errorMessage = null)
    {
        Organization = organization;
        ProjectId = buildPipeline.Project.Id;
        ProjectName = buildPipeline.Project.Name;
        PipelineId = buildPipeline.Id;
        PipelineName = buildPipeline.Name;
        Stages = stages;
        PipelineType = pipelineType;
        PipelineVersion = build.Definition.Revision;
        RunId = build.Id.ToString();
        RunUrl = build.Links.Web.Href.AbsoluteUri;
        StageId = stageId;
        BuildPipeline = buildPipeline;
        ErrorMessage = errorMessage;
    }

    public bool IsClassicPipeline =>
        PipelineType != null &&
        PipelineType.Equals(ItemTypes.ClassicReleasePipeline, StringComparison.InvariantCultureIgnoreCase);

    public bool IsProdStageRun(IEnumerable<string?>? prodStages) => 
        prodStages != null && prodStages.Any(
            stage => stage != null &&  stage.Equals(StageId, StringComparison.OrdinalIgnoreCase));
}