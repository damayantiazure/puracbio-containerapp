using Newtonsoft.Json;
using Rabobank.Compliancy.Domain.RuleProfiles;
using Rabobank.Compliancy.Infra.AzdoClient.Response.Interfaces;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class ReleaseDefinition : IRegisterableDefinition
{
    private IEnumerable<string> _stageIds;
    private IEnumerable<string> _registeredStageIds;
    private IEnumerable<Stage> _stages;
    public string Name { get; set; }
    public string Id { get; set; }
    public IList<ReleaseDefinitionEnvironment> Environments { get; set; }
    public IEnumerable<PipelineRegistration> PipelineRegistrations { get; set; }
    public TeamProjectReference ProjectReference { get; set; }
    public IList<Artifact> Artifacts { get; set; }
    public string Path { get; set; }
    public Dictionary<string, ConfigurationVariableValue> Variables { get; set; }
    public IdentityRef CreatedBy { get; set; }
    public IdentityRef ModifiedBy { get; set; }

    [JsonProperty("_links")]
    public Links Links { get; set; }

    public IEnumerable<string> GetStageIds()
    {
        if (_stageIds == null)
        {
            _stageIds = Environments.Select(s => s.Id.ToString());
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
        if (_stages == null)
        {
            _stages = Environments.Select(e => new Stage { Id = e.Id.ToString(CultureInfo.InvariantCulture), Name = e.Name });
        }
        return _stages;
    }

    public RuleProfile ProfileToApply { get; set; } = new DefaultRuleProfile();
}