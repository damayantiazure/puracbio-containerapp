using ExpectedObjects;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests.Integration.Requests;

public class AuditLogTests : IClassFixture<TestConfig>
{
    private readonly AzdoRestClient _client;

    public AuditLogTests(TestConfig config)
    {
        _client = new AzdoRestClient(config.Organization, config.Token);
    }

    [Fact(Skip = "integration test takes too long to be executed every time")]
    public async Task TestQueryContinuation()
    {
        var temp = await _client.GetAsync(AuditLog.Query());
        temp.Take(10).Count().ShouldBe(10);
    }

    [Fact(Skip = "integration test takes too long to be executed every time")]
    public async Task TestAuditLogEntry()
    {
        var expected = new
        {
            ActionId = Expect.NotDefault<string>(),
            Details = Expect.NotDefault<string>(),
            Area = Expect.NotDefault<string>(),
            Category = Expect.NotDefault<string>(),
            ProjectId = Expect.NotDefault<string>(),
            Timestamp = Expect.NotDefault<DateTime>()
        }.ToExpectedObject();

        var result = await _client.GetAsync(AuditLog.Query());
        result.Take(300).ShouldContain(e => expected.Matches(e));
    }

    [Fact]
    public async Task TestAuditLogEntryUsingStartAndEndDate()
    {
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var today = DateTime.UtcNow.Date;

        var result = await _client.GetAsync(AuditLog.Query(yesterday, today));
        result.ToList().ForEach(e => e.Timestamp.ShouldBeInRange(yesterday, today));
    }
}