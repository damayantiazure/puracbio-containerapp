#nullable enable

using Rabobank;
using Rabobank.Compliancy.Clients.AzureDataTablesClient.Deviations;
using Rabobank.Compliancy.Clients.AzureDataTablesClient.Exceptions;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Deviations;

namespace Rabobank.Compliancy.Infrastructure.Extensions;

public static class DeviationExtensions
{
    private const string IncorrectValueError = "Deviation with Rowkey {0} has an incorrect value for property {1}.";

    /// <summary>
    ///     Creates an instance of the <see cref="DeviationEntity" /> class by using the domain class <see cref="Deviation" />.
    /// </summary>
    /// <param name="deviation">
    ///     The domain model that contains information of the deviation. See <see cref="Deviation" />
    ///     class.
    /// </param>
    /// <param name="username">The user name that is used to update the table storage 'UpdatedBy' column.</param>
    /// <returns>A new instance of the <see cref="DeviationEntity" /> class.</returns>
    public static DeviationEntity ToEntity(this Deviation deviation, string? username) =>
        deviation.CreateDeviationEntity(username);

    /// <summary>
    ///     Creates an instance of the <see cref="DeviationEntity" /> class by using the domain class <see cref="Deviation" />.
    /// </summary>
    /// <param name="deviation">
    ///     The domain model that contains information of the deviation. See <see cref="Deviation" />
    ///     class.
    /// </param>
    /// <returns>A new instance of the <see cref="DeviationEntity" /> class.</returns>
    public static DeviationEntity ToDeleteEntity(this Deviation deviation) =>
        deviation.CreateDeviationEntity(null);

    private static DeviationEntity CreateDeviationEntity(this Deviation deviation, string? username) =>
        new(deviation.Project.Organization, deviation.Project.Name, deviation.RuleName,
            deviation.ItemId, deviation.CiIdentifier, deviation.Project.Id, deviation.Comment,
            deviation.Reason.ToString(), username, deviation.ItemProjectId)
        {
            Date = DateTime.UtcNow,
            ReasonNotApplicable = deviation.ReasonNotApplicable.ToString(),
            ReasonNotApplicableOther = deviation.ReasonNotApplicableOther,
            ReasonOther = deviation.ReasonOther
        };

    public static Deviation? ToDeviation(this DeviationEntity? deviationEntity)
    {
        if (deviationEntity == null)
        {
            return null;
        }

        if (deviationEntity.Reason == null)
        {
            throw new UnexpectedDataException(IncorrectValueError, deviationEntity.RowKey,
                nameof(deviationEntity.Reason));
        }

        if (string.IsNullOrEmpty(deviationEntity.ProjectId) ||
            !Guid.TryParse(deviationEntity.ProjectId, out var projectId))
        {
            throw new UnexpectedDataException(IncorrectValueError, deviationEntity.RowKey,
                nameof(deviationEntity.ProjectId));
        }

        var foreignProjectId = Guid.Empty;
        if (!string.IsNullOrEmpty(deviationEntity.ForeignProjectId) &&
            !Guid.TryParse(deviationEntity.ForeignProjectId, out foreignProjectId))
        {
            throw new UnexpectedDataException(IncorrectValueError, deviationEntity.RowKey,
                nameof(deviationEntity.ForeignProjectId));
        }

        if (deviationEntity.ItemId == null)
        {
            throw new UnexpectedDataException($"Cannot be null: {nameof(deviationEntity.ItemId)}");
        }

        if (deviationEntity.RuleName == null)
        {
            throw new UnexpectedDataException($"Cannot be null: {nameof(deviationEntity.RuleName)}");
        }

        if (deviationEntity.CiIdentifier == null)
        {
            throw new UnexpectedDataException($"Cannot be null: {nameof(deviationEntity.CiIdentifier)}");
        }

        if (deviationEntity.Comment == null)
        {
            throw new UnexpectedDataException($"Cannot be null: {nameof(deviationEntity.Comment)}");
        }

        var project = new Project
        {
            Name = deviationEntity.ProjectName,
            Id = projectId,
            Organization = deviationEntity.Organization
        };

        var reason = (DeviationReason)Enum.Parse(typeof(DeviationReason), deviationEntity.Reason);
        var reasonNotApplicable = string.IsNullOrEmpty(deviationEntity.ReasonNotApplicable)
            ? null
            : (DeviationApplicabilityReason?)Enum.Parse(typeof(DeviationApplicabilityReason),
                deviationEntity.ReasonNotApplicable);

        return new Deviation(deviationEntity.ItemId, deviationEntity.RuleName, deviationEntity.CiIdentifier, project,
            reason, reasonNotApplicable, deviationEntity.ReasonOther, deviationEntity.ReasonNotApplicableOther,
            deviationEntity.Comment)
        {
            ItemProjectId = foreignProjectId == Guid.Empty ? null : foreignProjectId,
            UpdatedBy = deviationEntity.UpdatedBy
        };
    }
}