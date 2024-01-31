namespace Rabobank.Compliancy.Domain.Enums;

/// <summary>
/// Enum that describes the different processtypes that Azure DevOps pipelines can have
/// </summary>
public enum PipelineProcessType
{
    Yaml,
    DesignerBuild,
    DesignerRelease,
    UnknownBuild
}