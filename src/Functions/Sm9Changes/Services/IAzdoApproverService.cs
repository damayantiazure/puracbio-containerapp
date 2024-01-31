#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Services;

public interface IAzdoApproverService
{
    Task<(IEnumerable<string>, IEnumerable<string>)> GetApproversAsync(string organization, string projectId, string runId);
}