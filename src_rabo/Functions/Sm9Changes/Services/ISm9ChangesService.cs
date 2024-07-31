#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Rabobank.Compliancy.Infra.Sm9Client.Change.Model;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Services;

public interface ISm9ChangesService
{
    void ValidateFunctionInput(HttpRequestMessage request, [NotNull] string? organization,
        [NotNull] string? projectId, [NotNull] string? pipelineType, [NotNull] string? runId);

    void ValidateFunctionInput(HttpRequestMessage request, [NotNull] string? organization,
        Guid projectId, [NotNull] string? pipelineType, int runId);

    Task<IEnumerable<ChangeInformation>> ValidateChangesAsync(IEnumerable<string> changeIds,
        IEnumerable<string> correctChangePhases,
        int validateChangeTimeOut);

    Task ApproveChangesAsync(string organization, IEnumerable<string> changeIds,
        IEnumerable<string> pipelineApprovers, IEnumerable<string> pullRequestApprovers);
}