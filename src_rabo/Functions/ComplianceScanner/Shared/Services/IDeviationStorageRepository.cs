using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;

public interface IDeviationStorageRepository
{
    public Task<IList<Deviation>> GetListAsync(string organization, string projectId);
    public Task UpdateAsync(Deviation deviation);
    public Task DeleteAsync(DynamicTableEntity deviation);
}