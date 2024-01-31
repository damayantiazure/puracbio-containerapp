using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Enums;

namespace Rabobank.Compliancy.Infrastructure.AzureDevOps;

/// <summary>
/// This class is intended to make use of Azure DevOps ReleaseDefinition specific properties
/// </summary>
public class AzdoReleaseDefinitionPipeline : Pipeline
{
    internal static PipelineProcessType Type => PipelineProcessType.DesignerRelease;
}