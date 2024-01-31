namespace Rabobank.Compliancy.Application.Enums;

/// <summary>
/// Enum for defining the different Artificat types
/// Other types are (but not yet used):'Jenkins', 'GitHub', 'Nuget', 'Team Build(external)', 'ExternalTFSBuild', 
/// 'TFVC', 'ExternalTfsXamlBuild'.
/// </summary>
public enum ArtifactType
{
    Build,
    Git
}