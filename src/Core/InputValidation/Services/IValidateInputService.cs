#nullable enable

using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace Rabobank.Compliancy.Core.InputValidation.Services;

public interface IValidateInputService
{
    void Validate([NotNull] HttpRequestMessage? request, [NotNull] string? organization, [NotNull] string? projectId);

    void Validate([NotNull] HttpRequestMessage? request, [NotNull] string? organization, [NotNull] string? projectId,
        [NotNull] string? itemId);

    void Validate([NotNull] string? organization, [NotNull] string? projectId, [NotNull] string? ciOrPipelineIdentifier,
        [NotNull] HttpRequestMessage? request);

    void Validate([NotNull] HttpRequestMessage? request, [NotNull] string? organization, [NotNull] string? projectId,
        [NotNull] string? ruleName, [NotNull] string? itemId);

    void Validate([NotNull] string? organization, [NotNull] string? projectId, [NotNull] string? runId,
        [NotNull] string? stageId, [NotNull] HttpRequestMessage? request);

    void Validate([NotNull] HttpRequestMessage? request, [NotNull] string? organization, [NotNull] string? projectId,
        [NotNull] string? ruleName, [NotNull] string? itemId, [NotNull] string? ciIdentifier);

    ActionResult ValidateInput(string? projectId, string? id, string? organizationUri,
        bool release);

    public void ValidateItemType(string itemType, string[] validTypes);
}