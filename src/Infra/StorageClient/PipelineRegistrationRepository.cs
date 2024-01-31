#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Rabobank.Compliancy.Infra.StorageClient.Model;

namespace Rabobank.Compliancy.Infra.StorageClient;

public class PipelineRegistrationRepository : IPipelineRegistrationRepository
{
    private readonly Lazy<CloudTableClient> _lazyClient;

    public PipelineRegistrationRepository(Func<CloudTableClient> factory) =>
        _lazyClient = new Lazy<CloudTableClient>(factory);

    public Task<List<PipelineRegistration>> GetAsync(string organization, string projectId)
    {
        if (organization == null)
        {
            throw new ArgumentNullException(nameof(organization));
        }

        if (projectId == null)
        {
            throw new ArgumentNullException(nameof(projectId));
        }

        return GetInternalAsync(organization, projectId);
    }

    public async Task<List<PipelineRegistration>> GetAsync(
        string organization, string projectId, string ciIdentifier) =>
        (await GetAsync(organization, projectId))
        .FilterByCiIdentifier(ciIdentifier)
        .ToList();

    public async Task<List<PipelineRegistration>> GetAsync(
        string organization, string projectId, string pipelineId, string stageId) =>
        (await GetAsync(organization, projectId))
        .FilterByPipeline(pipelineId, stageId)
        .ToList();

    public async Task<List<PipelineRegistration>> GetAsync(GetPipelineRegistrationRequest request) =>
        (await GetAsync(request.Organization, request.ProjectId))
        .FilterByPipelineType(request.PipelineId, request.PipelineType)
        .ToList();

    private async Task<List<PipelineRegistration>> GetInternalAsync(string organization, string projectId)
    {
        var query = new TableQuery<PipelineRegistration>().Where(TableQuery.CombineFilters(
            TableQuery.GenerateFilterCondition("Organization", QueryComparisons.Equal, organization),
            TableOperators.And,
            TableQuery.GenerateFilterCondition("ProjectId", QueryComparisons.Equal, projectId)));
        var table = _lazyClient.Value.GetTableReference("DeploymentMethod");

        if (!await table.ExistsAsync())
        {
            return new List<PipelineRegistration>();
        }

        var pipelineRegistrations = new List<PipelineRegistration>();
        TableContinuationToken? continuationToken = null;
        do
        {
            var page = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
            continuationToken = page.ContinuationToken;
            pipelineRegistrations.AddRange(page.Results);
        } while (continuationToken != null);

        return pipelineRegistrations
            .Distinct()
            .ToList();
    }
}