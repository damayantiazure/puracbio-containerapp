using Rabobank.Compliancy.Domain.RuleProfiles;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;

public interface IScanItemsService
{
    Task<IList<EvaluatedRule>> ScanProjectAsync(
        string organization, Project project, string ciIdentifier);
    Task<IEnumerable<EvaluatedRule>> ScanRepositoriesAsync(
        string organization, Project project, IEnumerable<Repository> repositories, string ciIdentifier);
    Task<IEnumerable<EvaluatedRule>> ScanBuildPipelinesAsync(
        string organization, Project project, IEnumerable<BuildDefinition> buildPipelines, string ciIdentifier,
        IEnumerable<RuleProfile> ruleProfilesForCi);
    Task<IEnumerable<EvaluatedRule>> ScanYamlReleasePipelinesAsync(
        string organization, Project project, IEnumerable<BuildDefinition> yamlReleasePipelines, string ciIdentifier);
    Task<IEnumerable<EvaluatedRule>> ScanClassicReleasePipelinesAsync(
        string organization, Project project, IEnumerable<ReleaseDefinition> classicReleasePipelines, string ciIdentifier);
}