using Rabobank.Compliancy.Domain.Enums;
using static Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model.Constants;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;

namespace Rabobank.Compliancy.Functions.PipelineBreaker.Model;

/// <summary>
///     The decorator verifies what the return message starts with:
///         No prefix -> Task succeeds
///         WARNING prefix -> A warning is thrown
///         ERROR prefix -> Pipeline is blocked
/// </summary>
public static class ResultMessages
{
    public static string Warned(string pipelineType, string errorMessage) =>
        Message(pipelineType, errorMessage, true);

    public static string Blocked(string pipelineType, string errorMessage) =>
        Message(pipelineType, errorMessage, false);

    private static string Message(string pipelineType, string errorMessage, bool warning)
    {
        var errorType = warning ? nameof(DecoratorPrefix.WARNING) : nameof(DecoratorPrefix.ERROR);

        if (pipelineType == ItemTypes.InvalidYamlPipeline)
        {
            return @$"{errorType}: { DecoratorResultMessages.InvalidYaml }
Error message: {errorMessage} ";
        }

        return $"{errorType}: { DecoratorResultMessages.NotRegistered }";
    }

    public static string AlreadyScanned(PipelineBreakerResult? result) =>
        result == PipelineBreakerResult.Passed
            ? DecoratorResultMessages.AlreadyScanned
            : DecoratorResultMessages.WarningAlreadyScanned;
}