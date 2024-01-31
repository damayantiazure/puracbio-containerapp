using Microsoft.Azure.WebJobs;
using Rabobank.Compliancy.Infra.Sm9Client.Cmdb;
using Rabobank.Compliancy.Infra.StorageClient;
using System.Linq;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Batch;

public class ImportRegistrationsFunction
{
    private readonly ICmdbClient _client;
    private readonly IPipelineRegistrationMapper _mapper;
    private readonly IPipelineRegistrationStorageRepository _repo;

    public ImportRegistrationsFunction(
        ICmdbClient client,
        IPipelineRegistrationMapper mapper,
        IPipelineRegistrationStorageRepository repo)
    {
        _client = client;
        _mapper = mapper;
        _repo = repo;
    }

    [FunctionName(nameof(ImportRegistrationsFunction))]
    public async Task RunAsync(
        [TimerTrigger("0 0 * * * *", RunOnStartup = false)] TimerInfo timerInfo)
    {
        var ciContentItems = await _client.GetAzDoCIsAsync();

        var registrations = ciContentItems
            .SelectMany(i => _mapper.Map(i))
            .ToList();

        await _repo.ImportAsync(registrations);
    }
}