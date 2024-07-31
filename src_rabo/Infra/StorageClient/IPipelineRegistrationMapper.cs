using System.Collections.Generic;
using Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;
using Rabobank.Compliancy.Infra.StorageClient.Model;

namespace Rabobank.Compliancy.Infra.StorageClient;

public interface IPipelineRegistrationMapper
{
    IEnumerable<PipelineRegistration> Map(CiContentItem ci);
}