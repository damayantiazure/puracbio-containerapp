using AutoFixture;
using AutoFixture.Kernel;
using Moq;
using Newtonsoft.Json.Linq;
using Rabobank.Compliancy.Core.Approvals.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Approvals.Tests.Services;

public class YamlReleaseApproverServiceTests
{
    private readonly IFixture _fixture = new Fixture();

    [Theory]
    [InlineData("GetStageApproversWithApprovers.json", true)]
    [InlineData("GetStageApproversWithSameApproverAsBuild.json", false)]
    [InlineData("GetStageApproversEmpty.json", false)]
    public async Task ApproverFileShouldGiveCorrectResponse(string jsonResponseFileName, bool hasApprovers)
    {
        // arrange
        var fixture = new Fixture();
        var client = await CreateRestClientAsync(fixture, jsonResponseFileName);

        // act
        var sut = new YamlReleaseApproverService(client);
        var result = await sut.HasApprovalAsync(_fixture.Create<Project>(), _fixture.Create<string>(),
            "Antoine.Geboers@rabobank.nl");

        //assert
        result.ShouldBe(hasApprovers);
    }

    [Fact]
    public async Task CanGetApprovers_FromMultistageYamlPipeline()
    {
        // arrange
        var fixture = new Fixture();
        var client = await CreateRestClientAsync(fixture, "GetStageApproversEmpty.json");

        // act
        var sut = new YamlReleaseApproverService(client);
        var actual = await sut.GetAllApproversAsync(fixture.Create<Project>(), fixture.Create<string>());

        // assert
        actual.ShouldBeEmpty();
    }

    [Fact]
    public async Task CanGetApprovers_FromMultistageYamlPipeline2()
    {
        // arrange
        var fixture = new Fixture();
        var client = await CreateRestClientAsync(fixture, "GetStageApproversWithApprovers.json");

        // act
        var sut = new YamlReleaseApproverService(client);
        var actual = await sut.GetAllApproversAsync(fixture.Create<Project>(), fixture.Create<string>());

        // assert
        actual.ShouldContain("Peter.Dol@rabobank.nl", "Rik.Brouwer@rabobank.com");
        actual.Count().ShouldBe(2);
    }

    [Fact]
    public async Task CanGetApprovers_FromMultistageYamlPipeline3()
    {
        // arrange
        var fixture = new Fixture();
        var client = await CreateRestClientAsync(fixture, "GetStageApproversWithSameApproverAsBuild.json");

        // act
        var sut = new YamlReleaseApproverService(client);
        var actual = await sut.GetAllApproversAsync(fixture.Create<Project>(), fixture.Create<string>());

        // assert
        actual.ShouldContain("Antoine.Geboers@rabobank.nl");
    }

    private static async Task<IAzdoRestClient> CreateRestClientAsync(
        ISpecimenBuilder fixture, string jsonResponseFileName)
    {
        var client = new Mock<IAzdoRestClient>();
        var timeline = fixture.Create<Timeline>();
        timeline.Records = new[]
        {
            new TimelineRecord
            {
                Id = Guid.NewGuid(),
                Name = "Prod",
                Type = "Stage"
            }
        };

        client
            .Setup(x => x.GetAsync(It.IsAny<IAzdoRequest<Timeline>>(), null))
            .ReturnsAsync(timeline);

        var responseFile = await File.ReadAllTextAsync(Path.Combine(
            "Assets", jsonResponseFileName));
        var json = JObject.Parse(responseFile);

        client
            .Setup(x => x.PostAsync(It.IsAny<IAzdoRequest<object, JObject>>(), It.IsAny<object>(), null, It.IsAny<bool>()))
            .ReturnsAsync(json);

        var project = fixture.Create<Project>();
        client
            .Setup(x => x.GetAsync(It.IsAny<IAzdoRequest<Project>>(), null))
            .ReturnsAsync(project);

        return client.Object;
    }
}