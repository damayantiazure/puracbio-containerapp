using Microsoft.TeamFoundation.Build.WebApi;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Enums;

namespace Rabobank.Compliancy.Infrastructure.Extensions;

internal static class BuildDefinitionExtensions
{
    internal static Pipeline ToPipelineObject(this BuildDefinition buildDefinition, Project project)
    {
        var pipeline = new Pipeline
        {
            Id = buildDefinition.Id,
            Name = buildDefinition.Name,
            DefinitionType = buildDefinition.Process.Type == ProcessType.Designer ? PipelineProcessType.DesignerBuild : PipelineProcessType.Yaml,
            Project = project,
            Runs = null,
            Path = buildDefinition.Path,
            DefaultRunContent = new PipelineBody
            {
                Triggers = buildDefinition.Triggers.OfType<BuildCompletionTrigger>().Select(x => new PipelineTrigger
                {
                    Id = x.Definition.Id,
                    ProjectId = x.Definition.Project.Id,
                    Organization = project.Organization
                })
            }
        };
        if (buildDefinition.Repository?.Type == "TfsGit" && Guid.TryParse(buildDefinition.Repository?.Id, out var repositoryId))
        {
            pipeline.DefaultRunContent.Resources = new[] {
                new GitRepo {
                    Id = repositoryId,
                    Name = buildDefinition.Repository.Name,
                    Project = project,
                    Url = buildDefinition.Repository.Url,
                    ConsumingPipelines = new[] { pipeline }
                }
            };
        }

        return pipeline;
    }
}