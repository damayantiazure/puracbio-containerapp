using System.Collections.Generic;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Model;

public static class SM9Constants
{
    public const string ChangeIdVarName = "ChangeId";
    public const string GetChangeIdRegex = "C[0-9]{9}";
    public const string ChangeIdRegex = "^C[0-9]{9}$";
    public const string ChangeIdWithUrlHashRegex = @"^C[0-9]{9} \[([a-fA-F0-9]{0,8})\]$";
    public const string MailRegex = "^(?!(eu\\.|fu\\.|fu_))(.*)\\@rabobank\\.(.*)$";
    public const string LowRiskChangeValue = "low";
    public const string BuildPipelineType = "build";
    public const string ReleasePipelineType = "release";
    public const string DeploymentPhase = "DEPLOYMENT";
    public const string ExecutionPhase = "EXECUTION";
    public const string ClosurePhase = "CLOSURE";
    public const string AbandonedPhase = "ABANDONED";
    public static readonly IEnumerable<string> CompletionCode = new[] { "1", "2", "3", "4", "5", "6" };
    public const string YamlLowRiskDocumentationLink = "https://confluence.dev.rabobank.nl/x/vDSgDw";
    public const string ClassicLowRiskDocumentationLink = "https://confluence.dev.rabobank.nl/x/NRV1D";
    public const string MediumHighRiskDocumentationLink = "https://confluence.dev.rabobank.nl/x/0hd9G";
}