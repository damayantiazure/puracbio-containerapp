using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Model;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Exceptions;

public static class ErrorMessages
{
    public static string InternalServerError(string exceptionMessage) =>
        @$"An unexpected error occurred.
Please try again and if it occurs more frequently please reach out to Tech4Dev.
Tech4Dev can be reached by creating a support ticket via https://tools.rabobank.nl.
Exception: {exceptionMessage}";

    public const string InternalServerErrorMessage =
        @"An unexpected error occurred.
Please try again and if it occurs more frequently please reach out to Tech4Dev.
Tech4Dev can be reached by creating a support ticket via https://tools.rabobank.nl.";

    public const string Unauthorized =
        @"The registration failed with an authorization error,
because the user has no permission to edit the pipeline.
Please make sure you have the correct permissions to register the pipeline.";

    public static string IsProductionItemError(ProductionDeployment deployment) =>
        @$"An error occurred while opening the permissions for this pipeline or repository.
This item has been part of the audit trail for production deployment(s) in the past 450 days.
Therefore, permissions cannot be opened.
The last deployment was on: {deployment.Date}.
The CI to which this deployment is linked was: {deployment.CiName}.
The pipeline run can be found here: {deployment.RunUrl}.";

    public static string ItemNotFoundError() =>
        @$"An error occurred while opening the permissions for this pipeline or repository.
This item could not be found and has probably already been removed from Azure DevOps.
Therefore, permissions cannot be opened.
Please rescan the project to make sure deleted items are removed from the overview.";

    public static string CiScanFailures(string ciIdentifier, string message) =>
        @$"One or more CI scans failed. All CI failures have been logged to Log Analytics.
The first failure was for CI: {ciIdentifier} with Exception: {message}.";

    public static string SessionInvalid(string message) =>
        $"Your browser session is invalid. Please refresh your browser. {message}";

    public static string CiDoesNotExist(string ciIdentifier) =>
        @$"The registration failed with a bad request error
because the Configuration Item {ciIdentifier} could not be found in SM9.
Please enter an existing CI.";

    public const string RegistrationUpdateUnAuthorized =
        "The update of your registration failed with an authorization error, " +
        "because the user has no permission to update the Configuration Item. " +
        "Please make sure you are a member of the assignment group that owns the CI.";

    public const string RegistrationDeleteUnAuthorized =
        "The deletion  of your registration failed with an authorization error, " +
        "because the user has no permission to update the Configuration Item. " +
        "Please make sure you are a member of the assignment group that owns the CI.";

    public const string ExistingRegistrationError =
        "The registration failed with a bad request error, " +
        "because an identical pipeline registration already exists. " +
        "Please use the rescan button to update your compliance page.";
}