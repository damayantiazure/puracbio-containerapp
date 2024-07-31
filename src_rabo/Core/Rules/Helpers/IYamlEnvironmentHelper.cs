using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Core.Rules.Helpers;

public interface IYamlEnvironmentHelper
{
    Task<IEnumerable<EnvironmentYaml>> GetProdEnvironmentsAsync(
        string organization, string projectId, BuildDefinition pipeline);
}