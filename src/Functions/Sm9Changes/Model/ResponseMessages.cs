using Rabobank.Compliancy.Functions.Sm9Changes.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Model;

public static class ResponseMessages
{
    public static string SuccessfullyApproved(IEnumerable<string> validChangeIds) =>
        $@"Task completed successfully.
Azure DevOps approvals and the pipeline url have been logged for changes: {validChangeIds.ToCommaSeparatedString()}.";

    public static string LowRiskChange =>
        $@"The change is classified as a low-risk change.
Therefore the verification of the change phase is skipped.";

    public static string CorrectPhase(IEnumerable<string> changeIds) =>
        $@"The verification is completed for changes: {changeIds.ToCommaSeparatedString()}.
All changes are in the correct phase: {SM9Constants.DeploymentPhase}.";

    public static string SuccessfullyClosed(IEnumerable<string> validChangeIds, 
        IEnumerable<string> alreadyClosed) =>
        alreadyClosed.Any()
            ? $@"Task completed successfully.
The following SM9 changes have been closed: {validChangeIds.ToCommaSeparatedString()}.
Ignored changeIds: {alreadyClosed.ToCommaSeparatedString()}. Changes were already closed."
            : $@"Task completed successfully.
The following SM9 changes have been closed: {validChangeIds.ToCommaSeparatedString()}.";
}