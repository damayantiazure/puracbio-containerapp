using Rabobank.Compliancy.Infra.StorageClient.Model;
using System.Collections.Generic;
using System.Linq;

namespace Rabobank.Compliancy.Infra.StorageClient;

public static class PipelineRegistrationExtensions
{
    public static string CiIdentifiers(this IEnumerable<PipelineRegistration> pipelineRegistrations) =>
        pipelineRegistrations.Select(x => x.CiIdentifier).ToCommaSeparatedString();

    public static string CiNames(this IEnumerable<PipelineRegistration> pipelineRegistrations) =>
        pipelineRegistrations.Select(x => x.CiName).ToCommaSeparatedString();

    public static bool IsSox(this IEnumerable<PipelineRegistration> pipelineRegistrations) =>
        pipelineRegistrations.Any(x => x.IsSoxApplication);

    public static IEnumerable<PipelineRegistration> FilterByPipeline(
        this IEnumerable<PipelineRegistration> pipelineRegistrations, string pipelineId, string stageId) =>
        pipelineRegistrations
            .Where(x => x.PipelineId == pipelineId && x.StageId != null && x.StageId.Equals(stageId, System.StringComparison.OrdinalIgnoreCase))
            .Distinct();

    public static IEnumerable<PipelineRegistration> FilterByPipelineType(
        this IEnumerable<PipelineRegistration> pipelineRegistrations, string pipelineId, string pipelineType) =>
        pipelineRegistrations.Where(x => x.PipelineId == pipelineId && x.PipelineType == pipelineType).Distinct();

    public static IEnumerable<PipelineRegistration> FilterByCiIdentifier(
        this IEnumerable<PipelineRegistration> pipelineRegistrations, string ciIdentifier) =>
        pipelineRegistrations.Where(x => x.CiIdentifier == ciIdentifier).Distinct();

    private static string ToCommaSeparatedString(this IEnumerable<string> values) =>
        string.Join(',', values);

    public static bool IsProduction(this string ciIdentifier) =>
        !string.IsNullOrEmpty(ciIdentifier);

    public static bool IsClassicReleasePipeline(this PipelineRegistration registration) =>
        IsClassicReleasePipeline(registration.StageId);

    public static bool IsClassicReleasePipeline(this string stageId) =>
        int.TryParse(stageId, out _);

    public static bool IsRegistered(this PipelineRegistration registration,
        string pipelineId, string pipelineType) =>
        registration.PipelineId == pipelineId &&
        registration.PipelineType == pipelineType;
}