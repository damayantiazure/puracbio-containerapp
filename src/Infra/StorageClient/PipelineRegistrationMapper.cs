using Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Rabobank.Compliancy.Infra.StorageClient.Tests")]

namespace Rabobank.Compliancy.Infra.StorageClient;

public class PipelineRegistrationMapper : IPipelineRegistrationMapper
{
    private const string YamlReleasePipeline = "YAML release";
    private const string ClassicReleasePipeline = "Classic release";

    public IEnumerable<PipelineRegistration> Map(CiContentItem ci) =>
        ci.Device?.DeploymentInfo == null
            ? Enumerable.Empty<PipelineRegistration>()
            : ci.Device.DeploymentInfo.Select(d => ParseSupplementaryInfo(d.SupplementaryInformation))
                .Where(s => s != null)
                .Select(s => CreateRegistrationEntity(s, ci));

    private static PipelineRegistration CreateRegistrationEntity(SupplementaryInformation supplementaryInformation, CiContentItem ci)
    {
        var configurationItem = MapToConfigurationItem(ci);
        return new PipelineRegistration(configurationItem, supplementaryInformation.Organization, supplementaryInformation.Project,
            supplementaryInformation.Pipeline, supplementaryInformation.Stage.IsClassicReleasePipeline()
                ? ClassicReleasePipeline
                : YamlReleasePipeline,
            supplementaryInformation.Stage,
            supplementaryInformation.Profile);
    }

    internal static SupplementaryInformation ParseSupplementaryInfo(string json)
    {
        return SupplementaryInformation.ParseSupplementaryInfo(json);
    }

    internal static ConfigurationItem MapToConfigurationItem(CiContentItem ci)
    {
        if (ci.Device == null)
        {
            return null;
        }
        var device = ci.Device;

        return new ConfigurationItem
        {
            AicClassification = device.BIVcode,
            CiID = device.CiIdentifier,
            CiName = device.DisplayName,
            CiType = device.ConfigurationItemType,
            CiSubtype = device.ConfigurationItemSubType,
            ConfigAdminGroup = device.AssignmentGroup,
            Environment = device.Environment,
            SOXClassification = device.SOXClassification,
            Status = device.Status
        };
    }
}