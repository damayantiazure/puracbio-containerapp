using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Kernel;
using Moq;
using Newtonsoft.Json.Linq;
using Rabobank.Compliancy.Core.Approvals.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Approvals.Tests.Services;

public class ClassicReleaseApproverServiceTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public async Task ShouldReturnAllValidApprovals()
    {
        // Arrange
        const string FirstApprover = "first.approver@rabobank.nl";
        const string InvalidApprover = "fu.second.approver@rabobank.nl";
        const string LastApprover = "last.approver@rabobank.nl";

        var releaseApprovals = new List<ReleaseApproval>

        {
            new ReleaseApproval
            {
                ApprovedBy = new IdentityRef
                {
                    UniqueName = FirstApprover
                },
                ModifiedOn = DateTime.MinValue
            },
            new ReleaseApproval
            {
                ApprovedBy = new IdentityRef
                {
                    UniqueName = InvalidApprover
                },
                ModifiedOn = DateTime.Now
            },
            new ReleaseApproval
            {
                ApprovedBy = new IdentityRef
                {
                    UniqueName = LastApprover
                },
                ModifiedOn = DateTime.MaxValue
            }
        };

        var client = new Mock<IAzdoRestClient>();
        client
            .Setup(x => x.GetAsync(It.IsAny<IEnumerableRequest<ReleaseApproval>>(), null))
            .ReturnsAsync(releaseApprovals);

        //Act
        var sut = new ClassicReleaseApproverService(client.Object);
        var result = await sut.GetAllApproversAsync(_fixture.Create<string>(), _fixture.Create<string>());

        //Assert
        result.Count().ShouldBe(2);
        Assert.Collection(result,
            s => Assert.Equal(expected: LastApprover, actual: s),
            s => Assert.Equal(expected: FirstApprover, actual: s));
    }

    [Fact]
    public async Task ShouldReturnUniqueApprovals()
    {
        // Arrange
        const string FirstApprover = "first.approver@rabobank.nl";
        const string LastApprover = "first.approver@rabobank.nl";

        var releaseApprovals = new List<ReleaseApproval>

        {
            new()
            {
                ApprovedBy = new IdentityRef
                {
                    UniqueName = FirstApprover
                },
                ModifiedOn = DateTime.MinValue
            },
            new()
            {
                ApprovedBy = new IdentityRef
                {
                    UniqueName = LastApprover
                },
                ModifiedOn = DateTime.MaxValue
            }
        };

        var client = new Mock<IAzdoRestClient>();
        client
            .Setup(x => x.GetAsync(It.IsAny<IEnumerableRequest<ReleaseApproval>>(), null))
            .ReturnsAsync(releaseApprovals);

        //Act
        var sut = new ClassicReleaseApproverService(client.Object);
        var result = await sut.GetAllApproversAsync(_fixture.Create<string>(), _fixture.Create<string>());

        //Assert
        result.Count().ShouldBe(1);
    }

    [Fact]
    public async Task ShouldReturnEmptyListWhenNoApproversFound()
    {
        // Arrange
        var client = new Mock<IAzdoRestClient>();
        client
            .Setup(x => x.GetAsync(It.IsAny<IEnumerableRequest<ReleaseApproval>>(), null))
            .ReturnsAsync(new List<ReleaseApproval>());

        //Act
        var sut = new ClassicReleaseApproverService(client.Object);
        var result = await sut.GetAllApproversAsync(_fixture.Create<string>(), _fixture.Create<string>());

        // Assert
        result.Any().ShouldBeFalse();
    }

    [Fact]
    public async Task CanGetApprovers_FromClassicPipeline()
    {
        // arrange
        var fixture = new Fixture();
        var client = await CreateRestClientAsync(fixture, "GetStageApproversEmpty.json");

        // act
        var sut = new ClassicReleaseApproverService(client);
        var actual = await sut.GetAllApproversAsync(fixture.Create<string>(), fixture.Create<string>());

        // assert
        actual.ShouldBeEmpty();
    }

    [Fact]
    public async Task ReleaseCreatorIsNotTheApproverShouldBeAllowed()
    {
        //Arrange
        var identityId1 = Guid.NewGuid();
        var identityId2 = Guid.NewGuid();

        _fixture.Customize<ReleaseApproval>(x => x
            .With(a => a.ApprovedBy, new IdentityRef { Id = identityId2, UniqueName = "jan.jansen@rabobank.com" })
            .With(a => a.IsAutomated, false));

        var client = new FixtureClient(_fixture);

        //Act
        var sut = new ClassicReleaseApproverService(client);
        var result = await sut.HasApprovalAsync(_fixture.Create<string>(), _fixture.Create<string>(), identityId1.ToString());

        //Assert
        result.ShouldBe(true);
    }

    [Fact]
    public async Task ReleaseCreatorIsTheApproverShouldNotBeAllowed()
    {
        //Arrange
        var identityId = Guid.NewGuid();

        _fixture.Customize<ReleaseApproval>(x => x
            .With(a => a.ApprovedBy, new IdentityRef { Id = identityId })
            .With(a => a.IsAutomated, false));

        var client = new FixtureClient(_fixture);

        //Act
        var sut = new ClassicReleaseApproverService(client);
        var result = await sut.HasApprovalAsync(_fixture.Create<string>(), _fixture.Create<string>(), identityId.ToString());

        //Assert
        result.ShouldBe(false);
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