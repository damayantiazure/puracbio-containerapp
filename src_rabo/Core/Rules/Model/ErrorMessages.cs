namespace Rabobank.Compliancy.Core.Rules.Model;

public static class ErrorMessages
{
    public static string InvalidClassicPipeline(string exceptionMessage) =>
        @$"Either this classic pipeline is invalid and could therefore not be updated.
Or the Project Collection Admin group lacks permissions to update the pipeline or stage.
Please make sure the classic pipeline is valid and the Project Collection Admin group has permission.
Exception: { exceptionMessage }";

    public static string InvalidYamlPipeline(string exceptionMessage) =>
        @$"This YAML pipeline is invalid and could therefore not be parsed.
Please make sure the YAML pipeline is valid before reconciling.
Exception: { exceptionMessage }";

    public static string NoEnvironment() =>
        $@"The production stage of this pipeline is not linked to an existing environment.
Therefore it is unknown on which environment the four eyes check must be added.
Please make sure the production stage is linked to an environment before reconciling.
For more information about environments see: 
https://docs.microsoft.com/en-us/azure/devops/pipelines/process/environments.";

    public static string InvalidEnvironments(string environmentNames) =>
        $@"Some of the environments to which the production stage of this pipeline is linked are invalid.
A runtime variable is used for the environment with this syntax: $(EnvironmentName).
The following Environment Names are found to be invalid: {environmentNames}.
Please make sure not to use runtime variables for environments, but use compile time variables instead.
For more information about variables see:
https://docs.microsoft.com/en-us/azure/devops/pipelines/process/variables.
For more information about environments see: 
https://docs.microsoft.com/en-us/azure/devops/pipelines/process/environments.";
}