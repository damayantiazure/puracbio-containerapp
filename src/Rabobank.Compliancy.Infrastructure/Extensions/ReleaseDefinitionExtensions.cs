using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Rabobank.Compliancy.Application.Enums;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Enums;

namespace Rabobank.Compliancy.Infrastructure.Extensions;

internal static class ReleaseDefinitionExtensions
{
    internal static Pipeline ToPipeline(this ReleaseDefinition releaseDefinition, Project project)
    {
        return releaseDefinition.ToPipeline(project, Enumerable.Empty<TaskGroup>());
    }

    internal static Pipeline ToPipeline(this ReleaseDefinition releaseDefinition, Project project, IEnumerable<TaskGroup> taskGroups)
    {
        var tasks = GetTasksFromPipeline(releaseDefinition, taskGroups);

        var pipeline = new Pipeline
        {
            Id = releaseDefinition.Id,
            Name = releaseDefinition.Name,
            DefinitionType = PipelineProcessType.DesignerRelease,
            Project = project,
            DefaultRunContent = new PipelineBody
            {
                Tasks = tasks,
                Stages = releaseDefinition.Environments.Select(e => new Stage
                {
                    Id = e.Id.ToString(),
                    Name = e.Name
                }),
                Gates = releaseDefinition.Environments.Select(e => new Gate
                {
                    Checks = releaseDefinition.ExtractChecks()
                })
            },
            Runs = null
        };
        pipeline.DefaultRunContent.Resources = releaseDefinition.ExtractGitRepos(project, pipeline);
        pipeline.DefaultRunContent.UsesNonBuildArtifact = releaseDefinition.Artifacts.Any(artifact => !artifact.Type.Equals(ArtifactType.Build.ToString(), StringComparison.OrdinalIgnoreCase));

        return pipeline;
    }

    internal static IEnumerable<AzureFunctionCheck> ExtractChecks(this ReleaseDefinition releaseDefinition)
    {
        var returnList = new List<AzureFunctionCheck>();
        foreach (var environment in releaseDefinition.Environments.Where(e =>
                     e.PreDeploymentGates != null && e.PreDeploymentGates.GatesOptions != null && e.PreDeploymentGates.Gates != null))
        {
            returnList.AddRange(environment.PreDeploymentGates.Gates.Where(gate => gate.Tasks != null).SelectMany(gate => gate.Tasks).Where(task => task.Inputs != null)
                .Select(task => new AzureFunctionCheck
                {
                    Function = task.Inputs.ContainsKey(nameof(AzureFunctionCheck.Function)) ? task.Inputs[nameof(AzureFunctionCheck.Function)] : null,
                    WaitForCompletion = task.Inputs.ContainsKey(nameof(AzureFunctionCheck.WaitForCompletion)) && task.Inputs[nameof(AzureFunctionCheck.WaitForCompletion)] == "true",
                    IsEnabled = environment.PreDeploymentGates.GatesOptions.IsEnabled
                }));
        }
        return returnList;
    }

    internal static List<GitRepo> ExtractGitRepos(this ReleaseDefinition releaseDefinition, Project project, Pipeline pipeline)
    {
        var gitRepos = new List<GitRepo>();
        foreach (var artifact in releaseDefinition.Artifacts.Where(artifact => artifact.Type.Equals(ArtifactType.Git.ToString(), StringComparison.OrdinalIgnoreCase)
                                                                               && artifact.DefinitionReference.ContainsKey("definition")))
        {
            if (!Guid.TryParse(artifact.DefinitionReference["definition"].Id, out var gitRepoId))
            {
                throw new InvalidOperationException($"Artifact of type \"Git\" does not seem to have a Git Repository ID that can be parsed as a Guid: {artifact.DefinitionReference["definition"].Id}");
            }

            gitRepos.Add(new GitRepo
            {
                Id = gitRepoId,
                ConsumingPipelines = new[] { pipeline },
                Name = artifact.DefinitionReference["definition"].Name,
                Project = project,
                Url = new Uri($"https://dev.azure.com/{project.Organization}/{project.Id}/_git/{gitRepoId}")
            });
        }

        return gitRepos;
    }

    private static IEnumerable<PipelineTask> GetTasksFromPipeline(ReleaseDefinition releaseDefinition, IEnumerable<TaskGroup> taskGroups)
    {
        var newTasksList = new List<PipelineTask>();

        foreach (var task in releaseDefinition.Environments.SelectMany(e => e.DeployPhases.SelectMany(d => d.WorkflowTasks)))
        {
            if (task.DefinitionType.Equals("Task", StringComparison.OrdinalIgnoreCase))
            {
                newTasksList.Add(new PipelineTask
                {
                    Id = task.TaskId,
                    Name = task.Name.StripNamespaceAndVersion(),
                    Inputs = task.Inputs
                });
                continue;
            }
            if (task.DefinitionType.Equals("MetaTask", StringComparison.OrdinalIgnoreCase))
            {
                var taskGroup = taskGroups.FirstOrDefault(taskGroup => taskGroup.Id == task.TaskId);
                if (taskGroup != null)
                {
                    foreach (var innerTask in taskGroup.Tasks)
                    {
                        newTasksList.Add(new PipelineTask
                        {
                            Id = innerTask.Task.Id,
                            Name = innerTask.DisplayName,
                            Inputs = innerTask.Inputs
                        });
                    }
                }
            }
        }

        return newTasksList;
    }
}