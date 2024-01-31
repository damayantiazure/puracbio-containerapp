using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Infra.AzdoClient.Processors;

public interface IQueryBatchProcessor
{
    Task<IEnumerable<WorkItemReference>> QueryByWiqlAsync(string organization, string project, string whereClause = null,
        int batchSize = 20_000 - 1);
}