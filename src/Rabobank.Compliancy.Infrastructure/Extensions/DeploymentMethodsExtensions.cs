using Rabobank.Compliancy.Clients.AzureDataTablesClient.DeploymentMethods;
using Rabobank.Compliancy.Clients.AzureDataTablesClient.Exceptions;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Registrations;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.RuleProfiles;

namespace Rabobank.Compliancy.Infrastructure.Extensions;

public static class DeploymentMethodsExtensions
{
    private const string IncorrectValueError =
        "DeploymentMethod with Rowkey {0} has an incorrect value for property {1}.";

    private const string YamlReleaseRegistration = "YAML release";
    private const string ClassicReleaseRegistration = "Classic release";

    public static IEnumerable<NonProdPipelineRegistration> ToNonProdPipelineRegistrations(
        this IEnumerable<DeploymentMethodEntity> entities) =>
        entities.Select(ToNonProdPipelineRegistration);

    public static NonProdPipelineRegistration ToNonProdPipelineRegistration(this DeploymentMethodEntity entity)
    {
        if (string.IsNullOrEmpty(entity.PipelineId) || !int.TryParse(entity.PipelineId, out var pipelineId) ||
            pipelineId == default)
        {
            throw new UnexpectedDataException(IncorrectValueError, entity.RowKey, nameof(entity.PipelineId));
        }

        if (string.IsNullOrEmpty(entity.PipelineType))
        {
            throw new UnexpectedDataException(IncorrectValueError, entity.RowKey, nameof(entity.PipelineType));
        }

        if (string.IsNullOrEmpty(entity.ProjectId) || !Guid.TryParse(entity.ProjectId, out var projectId) ||
            projectId == Guid.Empty)
        {
            throw new UnexpectedDataException(IncorrectValueError, entity.RowKey, nameof(entity.ProjectId));
        }

        return new NonProdPipelineRegistration
        {
            RuleProfile = RuleProfile.GetProfile(entity.RuleProfileName),
            Pipeline = new Pipeline
            {
                Id = pipelineId,
                DefinitionType = entity.PipelineType.ToPipelineProcessType(),
                Project = new Project
                {
                    Id = projectId,
                    Organization = entity.Organization
                }
            },
            ShouldBeScanned = entity.ToBeScanned == true,
            StageId = entity.StageId
        };
    }

    private static PipelineProcessType ToPipelineProcessType(this string registrationType)
    {
        if (registrationType.Equals(YamlReleaseRegistration, StringComparison.OrdinalIgnoreCase))
        {
            return PipelineProcessType.Yaml;
        }

        if (registrationType.Equals(ClassicReleaseRegistration, StringComparison.OrdinalIgnoreCase))
        {
            return PipelineProcessType.DesignerRelease;
        }

        throw new ArgumentException("RegistrationType {registrationType} cannot be mapped to processType.");
    }
}