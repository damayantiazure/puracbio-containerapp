using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests.Integration.Requests;

[Trait("category", "integration")]
public class DistributedTasksTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;

    public DistributedTasksTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(config.Organization, config.Token);
    }

    [Fact]
    public async Task GetAllOrganizationalPools()
    {
        var orgPools = await _client.GetAsync(DistributedTask.OrganizationalAgentPools());
        orgPools.ToList();

        orgPools.ShouldAllBe(_ => !string.IsNullOrEmpty(_.Name));
        orgPools.ShouldAllBe(_ => !string.IsNullOrEmpty(_.PoolType));
    }

    [Fact]
    public async Task GetAgentPool()
    {
        var orgPools = await _client.GetAsync(DistributedTask.OrganizationalAgentPools());
        orgPools.ToList();
        var agentPool = await _client.GetAsync(DistributedTask.AgentPool(orgPools.First().Id));
        agentPool.Name.ShouldBe(orgPools.First().Name);
    }

    [Fact]
    public async Task GetAgentStatus()
    {
        var orgPools = await _client.GetAsync(DistributedTask.OrganizationalAgentPools());
        orgPools.ToList();
        var agentStatus = await _client.GetAsync(DistributedTask.AgentPoolStatus(orgPools.First().Id));
        agentStatus.ShouldAllBe(_ => !string.IsNullOrEmpty(_.Name));
    }

    [Fact]
    public async Task GetTask()
    {
        var task = await _client.GetAsync(DistributedTask.Tasks());
        task.ShouldAllBe(_ => !string.IsNullOrWhiteSpace(_.Id));
    }

    [Fact]
    public async Task QueryAgentQueueTest()
    {
        var response = await _client.GetAsync(DistributedTask.AgentQueue(_config.ProjectName, 3542));
        response.Id.ShouldBe(3542);
        response.Pool.ShouldNotBeNull();
        response.Pool.Id.ShouldBe(68);
    }

    [Fact]
    public async Task QueryAgentQueueThatDoesNotExistTest()
    {
        var response = await _client.GetAsync(DistributedTask.AgentQueue(_config.ProjectName, 123));
        response.ShouldBeNull();
    }
}