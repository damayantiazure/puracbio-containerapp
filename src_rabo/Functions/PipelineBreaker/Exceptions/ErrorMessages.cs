using System;
using static Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model.Constants;

namespace Rabobank.Compliancy.Functions.PipelineBreaker.Exceptions;

public static class ErrorMessages
{
    public static string InternalServerErrorMessage() =>
        @$"{DecoratorErrors.ErrorPrefix}An internal server error occurred while executing the compliance scan for this pipeline run. 
As this pipeline run could not be validated, it is allowed to continue.";

    public static string BuildNotAvailableErrorMessage(string runId) =>
        $"The pipelinebreaker application account does not have the correct permissions to retrieve build {runId} " +
        "or the build does not exist.";

    public static string ReleaseNotAvailableErrorMessage(string releaseId) =>
        $"The pipelinebreaker application account does not have the correct permissions to retrieve release {releaseId} " +
        "or the release does not exist.";
}