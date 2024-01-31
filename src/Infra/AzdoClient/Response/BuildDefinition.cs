using Newtonsoft.Json;
using Rabobank.Compliancy.Domain.RuleProfiles;
using Rabobank.Compliancy.Infra.AzdoClient.Response.Interfaces;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public sealed class BuildDefinition : IEquatable<BuildDefinition>, IRegisterableDefinition
{
    private IEnumerable<string> _stageIds;
    private IEnumerable<string> _registeredStageIds;

    public string Id { get; set; }
    public string Name { get; set; }
    public BuildProcess Process { get; set; }
    public TeamProjectReference Project { get; set; }
    public Repository Repository { get; set; }
    public string Path { get; set; }
    public string QueueStatus { get; set; }
    public IdentityRef AuthoredBy { get; set; }
    public IEnumerable<Triggers> Triggers { get; set; }

    // The following four properties are not received from the AzDO API.
    // They are used to enrich the Build Definition to reuse YAML data.
    public string PipelineType { get; set; }

    public string Yaml { get; set; }
    public string YamlUsedInRun { get; set; }
    public IEnumerable<Stage> Stages { get; set; }
    public IEnumerable<PipelineRegistration> PipelineRegistrations { get; set; }

    [JsonProperty("_links")]
    public Links Links { get; set; }

    public override bool Equals(object obj)
    {
        return Equals(obj as BuildDefinition);
    }

    public bool Equals([AllowNull] BuildDefinition other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Id.Equals(other.Id) && ProfileToApply.Equals(other.ProfileToApply);
    }

    public string UsedYaml => YamlUsedInRun ?? Yaml; // In case we are evaluating a run, we want the yaml used by that run

    public bool IsValidInput() =>
        string.IsNullOrEmpty(Yaml) && string.IsNullOrEmpty(YamlUsedInRun) &&
        (PipelineType != ItemTypes.InvalidYamlPipeline || PipelineType != ItemTypes.DisabledYamlPipeline);

    public override int GetHashCode() => Id.GetHashCode();

    public IEnumerable<string> GetStageIds()
    {
        if (_stageIds == null)
        {
            _stageIds = Stages.Select(s => s.Id);
        }
        return _stageIds;
    }

    public IEnumerable<string> GetRegisteredStageIds()
    {
        if (_registeredStageIds == null)
        {
            _registeredStageIds = PipelineRegistrations.Select(registration => registration.StageId);
        }
        return _registeredStageIds;
    }

    public IEnumerable<Stage> GetStages()
    {
        return Stages;
    }

    public RuleProfile ProfileToApply { get; set; } = new DefaultRuleProfile();
}