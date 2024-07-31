using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests.Integration.Requests;

public class TaskGroupTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;

    public TaskGroupTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(_config.Organization, _config.Token);
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task GetTaskGroupById()
    {
        var response = await _client.GetAsync(TaskGroup.TaskGroupById(_config.ProjectName, _config.TaskGroupId));
        response.ShouldNotBeNull();
        response.Value.FirstOrDefault().Tasks.ShouldNotBeNull();
        response.Value.FirstOrDefault().Tasks.FirstOrDefault().Task.Id.ShouldNotBeNull();
    }
}