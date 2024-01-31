using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Enums;

namespace Rabobank.Compliancy.Infrastructure.AzureDevOps;

/// <summary>
/// This class is intended to make use of Azure DevOps BuildDefinition specific properties, such as stating whether it is a DesignerBuild, or Yml pipeline type
/// </summary>
public class AzdoBuildDefinitionPipeline : Pipeline
{
    internal readonly PipelineProcessType? Type;
}