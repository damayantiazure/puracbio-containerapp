#nullable enable

using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Clients.AzureDataTablesClient.DeploymentMethods;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Registrations;
using Rabobank.Compliancy.Infrastructure.Extensions;
using TableStorage.Abstractions.Store;

namespace Rabobank.Compliancy.Infrastructure;

/// <inheritdoc />
public class PipelineRegistrationService : IPipelineRegistrationService
{
    private readonly Lazy<ITableStore<DeploymentMethodEntity>> _lazyRepository;

    /// <summary>
    ///     Constructor intended only for dependency injection
    /// </summary>
    /// <param name="factory">
    ///     Injected using the
    ///     <see href="https://github.com/Tazmainiandevil/TableStorage.Abstractions">TableStorage.Abstractions</see> package
    /// </param>
    public PipelineRegistrationService(Func<ITableStore<DeploymentMethodEntity>> factory) =>
        _lazyRepository = new Lazy<ITableStore<DeploymentMethodEntity>>(factory);

    /// <inheritdoc />
    public async Task<IEnumerable<NonProdPipelineRegistration>> GetNonProdPipelineRegistrationsForProjectAsync(
        Project project, CancellationToken cancellationToken = default)
    {
        var registrationEntities = await _lazyRepository.Value.GetRecordsByFilterAsync(deploymentMethodEntity =>
            deploymentMethodEntity.ProjectId == project.Id.ToString()
            && string.IsNullOrEmpty(deploymentMethodEntity.CiIdentifier), 0, -1, cancellationToken);

        return registrationEntities.ToNonProdPipelineRegistrations();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<NonProdPipelineRegistration>> GetNonProdPipelineRegistrationsForPipelineAsync(
        Pipeline pipeline, CancellationToken cancellationToken = default)
    {
        var registrationEntities =
            await _lazyRepository.Value.GetRecordsByFilterAsync(deploymentMethodEntity => deploymentMethodEntity.PipelineId == pipeline.Id.ToString(), 0, -1,
                cancellationToken);

        return registrationEntities.ToNonProdPipelineRegistrations();
    }
}