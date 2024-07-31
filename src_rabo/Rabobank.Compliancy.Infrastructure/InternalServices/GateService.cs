using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Infrastructure.InternalContracts;

namespace Rabobank.Compliancy.Infrastructure.InternalServices;

/// <inheritdoc/>
internal class GateService : IGateService
{
    private readonly IEnvironmentRepository _environmentRepository;
    private readonly ICheckConfigurationRepository _checkConfigurationRepository;

    private readonly Dictionary<Guid, IEnumerable<EnvironmentInstance>> _environmentsPerProject = new();

    private readonly Dictionary<string, IEnumerable<Gate>> _gatesPerBuildDefinition = new();
    private readonly Dictionary<string, IEnumerable<AzureFunctionCheck>> _checksPerEnvironment = new();

    public GateService(IEnvironmentRepository environmentRepository, ICheckConfigurationRepository checkConfigurationRepository)
    {
        _environmentRepository = environmentRepository;
        _checkConfigurationRepository = checkConfigurationRepository;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Gate>> GetGatesForBuildDefinitionAsync(Project project,
        int buildDefinitionId, IEnumerable<string> environmentNames, CancellationToken cancellationToken = default)
    {
        if (_gatesPerBuildDefinition.ContainsKey($"{project.Id}{buildDefinitionId}"))
        {
            return _gatesPerBuildDefinition[$"{project.Id}{buildDefinitionId}"];
        }

        var environments = await GetEnvironmentByProjectFromCache(project, cancellationToken);
        var filteredEnvironments = environments?.Where(e => environmentNames.Contains(e.Name)).ToList();  // Must be mutable, therefore a list, to add checks later.
        var gatesListForThisBuildDefinition = new List<Gate>();

        foreach (var environment in filteredEnvironments ?? Enumerable.Empty<EnvironmentInstance>())
        {
            // Here we try to get the checks from a cached field for this environment.
            // This seems redundant (because if the checks have been added to the environment before, they
            // will still be there), but it's not. Because how will we differentiate between the situation
            // where there are no checks and the situation where the checks haven't already been collected?
            //
            // We can't, so that's why we put them in a dictionary. If the key exists, we know they've
            // already been collected, even if the value of that pair is null.
            if (!_checksPerEnvironment.ContainsKey($"{project.Id}{environment.Id}"))
            {
                var checks = await _checkConfigurationRepository.GetCheckConfigurationsForEnvironmentAsync(project.Organization, project.Id, environment.Id, cancellationToken);
                if (checks == null)
                {
                    _checksPerEnvironment.Add($"{project.Id}{environment.Id}", null);
                    continue;
                }
                _checksPerEnvironment.Add($"{project.Id}{environment.Id}", checks.Where(c => c.Settings != null && c.Settings.Inputs != null).Select(c => new AzureFunctionCheck
                {
                    Function = c.Settings.Inputs.Function,
                    IsEnabled = true,
                    Method = c.Settings.Inputs.Method,
                    WaitForCompletion = c.Settings.Inputs.WaitForCompletion == true
                }));
            }

            gatesListForThisBuildDefinition.Add(new Gate { Checks = _checksPerEnvironment[$"{project.Id}{environment.Id}"] });
        }

        _gatesPerBuildDefinition.Add($"{project.Id}{buildDefinitionId}", gatesListForThisBuildDefinition);

        return gatesListForThisBuildDefinition;
    }

    private async Task<IEnumerable<EnvironmentInstance>> GetEnvironmentByProjectFromCache(Project project, CancellationToken cancellationToken = default)
    {
        if (_environmentsPerProject.ContainsKey(project.Id))
        {
            return _environmentsPerProject[project.Id];
        }

        var environmentInstances = await _environmentRepository.GetEnvironmentsAsync(project.Organization, project.Id, cancellationToken);

        _environmentsPerProject.Add(project.Id, environmentInstances);

        return environmentInstances;
    }
}