using Rabobank.Compliancy.Application.Helpers.Extensions;
using Rabobank.Compliancy.Domain.Compliancy;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Rabobank.Compliancy.Infrastructure.Tests")]

namespace Rabobank.Compliancy.Infrastructure.Models;

internal abstract class PipelineReferenceTask
{
    protected PipelineTask _task;

    protected PipelineReferenceTask(PipelineTask task, string[] projectRefFieldNames, string[] pipelineRefFieldNames)
    {
        _task = task;
        ProjectRefFieldNames = projectRefFieldNames;
        PipelineRefFieldNames = pipelineRefFieldNames;
    }
    public string TaskName { get; protected set; }
    public Guid Id { get; protected set; }

    public string[] ProjectRefFieldNames { get; private set; }
    public string[] PipelineRefFieldNames { get; private set; }

    public Guid? ReferencedProject
    {
        get
        {
            var projectValue = _task.Inputs?.FirstOrDefault(x => ProjectRefFieldNames.Contains(x.Key, StringComparer.OrdinalIgnoreCase)).Value;
            if (string.IsNullOrEmpty(projectValue?.Trim()))
            {
                return null;
            }

            return projectValue.ToGuidOrDefault();
        }
    }

    public int? ReferencedPipelineId
    {
        get
        {
            var pipelineIdValue = _task.Inputs?.FirstOrDefault(x => PipelineRefFieldNames.Contains(x.Key, StringComparer.OrdinalIgnoreCase)).Value;
            if (string.IsNullOrEmpty(pipelineIdValue?.Trim()))
            {
                return null;
            }

            return pipelineIdValue.ToIntegerOrDefault();
        }
    }
}