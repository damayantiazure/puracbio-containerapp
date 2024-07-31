using Rabobank.Compliancy.Domain.Enums;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Helpers;
public static class PipelineProcessTypeExtensions
{
    public static int ToAzureDevopsInt(this PipelineProcessType processType)
    {
        /*this is used for build pipelines only, so the rest should be 0 */
        if (processType == PipelineProcessType.DesignerBuild)
        {
            return 1;
        }
        else if (processType == PipelineProcessType.Yaml)
        {
            return 2;
        }
        else
        {
            return 0;
        }
    }
}
