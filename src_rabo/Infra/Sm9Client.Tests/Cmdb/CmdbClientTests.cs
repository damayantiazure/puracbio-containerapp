using Rabobank.Compliancy.Infra.Sm9Client.Cmdb;
using Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

namespace Rabobank.Compliancy.Infra.Sm9Client.Tests.Cmdb;

public class CmdbClientTests
{
    private const string ManageDeploymentInformationEndpoint = "/managedeploymentinformation";
    private const string RetrieveCiInfoByKeyEndpoint = "/retrieveCiInfoByKey";
    private const string RetrieveGroupInfoByKeyEndpoint = "/retrieveGroupInfoByKey";
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task GetCiAsync_CallToHttpClient_ShouldBeSuccessful()
    {
        // Arrange
        var ciIdentifier = _fixture.Create<string>();
        var httpMethod = HttpMethod.Post;

        var responseObj = _fixture.Build<RetrieveCiByKeyResponse>()
            .With(f => f.CiInfo, _fixture.Build<CiInfo>()
                .With(f => f.ConfigurationItem, new[]
                {
                    _fixture.Build<ConfigurationItem>()
                        .With(f => f.CiID, ciIdentifier)
                        .Create()
                })
                .Create())
            .Create();

        var messageHandlerMock = TestHelpers.SetupSuccessHttpMessageHandlerMock<RetrieveCiByKeyRequest>(
            RetrieveCiInfoByKeyEndpoint,
            httpMethod, content => content.Body!.Key!.Single() == ciIdentifier, responseObj);

        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), messageHandlerMock.Object);

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = await sut.GetCiAsync(ciIdentifier);

        // Assert
        messageHandlerMock.Verify();
        actual!.CiID.Should().Be(responseObj.CiInfo!.ConfigurationItem!.Single().CiID);
    }

    [Fact]
    public async Task GetCiAsync_CallToHttpClient_ShouldFail()
    {
        // Arrange
        var ciIdentifier = _fixture.Create<string>();
        var httpMethod = HttpMethod.Post;

        var messageHandlerMock = TestHelpers.SetupFailHttpMessageHandlerMock(RetrieveCiInfoByKeyEndpoint, httpMethod);
        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), messageHandlerMock.Object);

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = () => sut.GetCiAsync(ciIdentifier);

        // Assert
        await actual.Should().ThrowAsync<HttpRequestException>();
        messageHandlerMock.Verify();
    }

    [Fact]
    public async Task GetCiAsync_WithWithMultipleConfigurationItems_ShouldThrowException()
    {
        // Arrange
        var ciIdentifier = _fixture.Create<string>();
        var httpMethod = HttpMethod.Post;

        var responseObj = _fixture.Build<RetrieveCiByKeyResponse>()
            .With(f => f.CiInfo, _fixture.Build<CiInfo>()
                .With(f => f.ConfigurationItem,
                    _fixture.Build<ConfigurationItem>()
                        .With(f => f.CiID, ciIdentifier)
                        .CreateMany
                )
                .Create())
            .Create();

        var messageHandlerMock =
            TestHelpers.SetupSuccessHttpMessageHandlerMock(RetrieveCiInfoByKeyEndpoint, httpMethod, responseObj);

        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), messageHandlerMock.Object);

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = () => sut.GetCiAsync(ciIdentifier);

        // Assert
        await actual.Should().ThrowExactlyAsync<InvalidOperationException>();
        messageHandlerMock.Verify();
    }

    [Fact]
    public async Task GetCiAsync_WithNoCiInfo_ShouldReturnNull()
    {
        // Arrange
        var ciIdentifier = _fixture.Create<string>();
        var httpMethod = HttpMethod.Post;

        var responseObj = _fixture.Build<RetrieveCiByKeyResponse>()
            .Without(f => f.CiInfo)
            .Create();

        var messageHandlerMock =
            TestHelpers.SetupSuccessHttpMessageHandlerMock(RetrieveCiInfoByKeyEndpoint, httpMethod, responseObj);

        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), messageHandlerMock.Object);

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = await sut.GetCiAsync(ciIdentifier);

        // Assert
        messageHandlerMock.Verify();
        actual.Should().BeNull();
    }

    [Fact]
    public async Task GetAssignmentGroupAsync_CallToHttpClient_ShouldBeSuccessful()
    {
        // Arrange
        var assignmentGroupName = _fixture.Create<string>();
        var groupMembers = _fixture.CreateMany<string>();
        var httpMethod = HttpMethod.Post;

        var responseObj = _fixture.Build<RetrieveGroupByKeyResponse>()
            .With(f => f.GroupInfo, _fixture.Build<GroupInfo>()
                .With(f => f.AssignmentGroup, new[]
                {
                    _fixture.Build<AssignmentGroup>()
                        .With(f => f.GroupMembers, groupMembers)
                        .Create()
                })
                .Create())
            .Create();

        var messageHandlerMock = TestHelpers.SetupSuccessHttpMessageHandlerMock<RetrieveGroupByKeyRequest>(
            RetrieveGroupInfoByKeyEndpoint,
            httpMethod, content => content.Body!.Key!.Single() == assignmentGroupName, responseObj);

        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), messageHandlerMock.Object);

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = await sut.GetAssignmentGroupAsync(assignmentGroupName);

        // Assert
        messageHandlerMock.Verify();
        actual!.GroupMembers.Should().BeEquivalentTo(responseObj.GroupInfo!.AssignmentGroup!.Single().GroupMembers);
    }

    [Fact]
    public async Task GetAssignmentGroupAsync_CallToHttpClient_ShouldFail()
    {
        // Arrange
        var assignmentGroupName = _fixture.Create<string>();
        var httpMethod = HttpMethod.Post;

        var messageHandlerMock =
            TestHelpers.SetupFailHttpMessageHandlerMock(RetrieveGroupInfoByKeyEndpoint, httpMethod);
        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), messageHandlerMock.Object);

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = () => sut.GetAssignmentGroupAsync(assignmentGroupName);

        // Assert
        await actual.Should().ThrowAsync<HttpRequestException>();
        messageHandlerMock.Verify();
    }

    [Fact]
    public async Task GetAssignmentGroupAsync_WithWithMultipleAssignmentGroup_ShouldThrowException()
    {
        // Arrange
        var assignmentGroupName = _fixture.Create<string>();
        var httpMethod = HttpMethod.Post;

        var responseObj = _fixture.Create<RetrieveGroupByKeyResponse>();

        var messageHandlerMock =
            TestHelpers.SetupSuccessHttpMessageHandlerMock(RetrieveGroupInfoByKeyEndpoint, httpMethod, responseObj);

        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), messageHandlerMock.Object);

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = () => sut.GetAssignmentGroupAsync(assignmentGroupName);

        // Assert
        await actual.Should().ThrowExactlyAsync<InvalidOperationException>();
        messageHandlerMock.Verify();
    }

    [Fact]
    public async Task GetAssignmentGroupAsync_WithoutAssignmentGroup_ShouldReturnNull()
    {
        // Arrange
        var assignmentGroupName = _fixture.Create<string>();
        var httpMethod = HttpMethod.Post;

        var responseObj = _fixture.Build<RetrieveGroupByKeyResponse>()
            .With(f => f.GroupInfo, _fixture.Build<GroupInfo>()
                .Without(f => f.AssignmentGroup)
                .Create())
            .Create();

        var messageHandlerMock = TestHelpers.SetupSuccessHttpMessageHandlerMock<RetrieveGroupByKeyRequest>(
            RetrieveGroupInfoByKeyEndpoint,
            httpMethod, content => content.Body!.Key!.Single() == assignmentGroupName, responseObj);

        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), messageHandlerMock.Object);

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = await sut.GetAssignmentGroupAsync(assignmentGroupName);

        // Assert
        messageHandlerMock.Verify();
        actual.Should().BeNull();
    }

    [Fact]
    public async Task GetAssignmentGroupAsync_WithoutGroupInfo_ShouldReturnNull()
    {
        // Arrange
        var assignmentGroupName = _fixture.Create<string>();
        var httpMethod = HttpMethod.Post;

        var responseObj = _fixture.Build<RetrieveGroupByKeyResponse>()
            .Without(f => f.GroupInfo)
            .Create();

        var messageHandlerMock = TestHelpers.SetupSuccessHttpMessageHandlerMock<RetrieveGroupByKeyRequest>(
            RetrieveGroupInfoByKeyEndpoint,
            httpMethod, content => content.Body!.Key!.Single() == assignmentGroupName, responseObj);

        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), messageHandlerMock.Object);

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = await sut.GetAssignmentGroupAsync(assignmentGroupName);

        // Assert
        messageHandlerMock.Verify();
        actual.Should().BeNull();
    }

    [Fact]
    public async Task GetDeploymentMethodAsync_CallToHttpClient_ShouldBeSuccessful()
    {
        // Arrange
        var ciName = _fixture.Create<string>();
        var information = _fixture.Create<string>();
        var method = _fixture.Create<string>();
        var httpMethod = HttpMethod.Post;

        var responseObj = _fixture.Build<ManageDeploymentInformationResponse>()
            .With(f => f.ManageDeploymentInformation, _fixture.Build<ManageDeploymentInformationDeploymentInfo>()
                .With(f => f.DeploymentInformations, new[]
                {
                    _fixture.Build<DeploymentInformation>()
                        .With(f => f.Information, information)
                        .With(f => f.Method, method)
                        .Create()
                })
                .Create())
            .Create();

        var messageHandlerMock = TestHelpers.SetupSuccessHttpMessageHandlerMock<ManageDeploymentInformationRequest>(
            ManageDeploymentInformationEndpoint, httpMethod, content => content.Body!.Key == ciName, responseObj);

        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), messageHandlerMock.Object);

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = (await sut.GetDeploymentMethodAsync(ciName))!.Single();

        // Assert
        messageHandlerMock.Verify();
        actual.Information.Should().Be(information);
        actual.Method.Should().Be(method);
    }

    [Fact]
    public async Task GetDeploymentMethodAsync_WithoutManageDeploymentInformation_ShouldReturnNull()
    {
        // Arrange
        var ciName = _fixture.Create<string>();
        var httpMethod = HttpMethod.Post;

        var responseObj = _fixture.Build<ManageDeploymentInformationResponse>()
            .Without(f => f.ManageDeploymentInformation)
            .Create();

        var messageHandlerMock = TestHelpers.SetupSuccessHttpMessageHandlerMock<ManageDeploymentInformationRequest>(
            ManageDeploymentInformationEndpoint, httpMethod, content => content.Body!.Key == ciName, responseObj);

        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), messageHandlerMock.Object);

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = await sut.GetDeploymentMethodAsync(ciName);

        // Assert
        messageHandlerMock.Verify();
        actual.Should().BeNull();
    }

    [Fact]
    public async Task GetDeploymentMethodAsync_CallToHttpClient_ShouldFail()
    {
        // Arrange
        var ciName = _fixture.Create<string>();
        var httpMethod = HttpMethod.Post;

        var messageHandlerMock =
            TestHelpers.SetupFailHttpMessageHandlerMock(ManageDeploymentInformationEndpoint, httpMethod);
        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), messageHandlerMock.Object);

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = () => sut.GetDeploymentMethodAsync(ciName);

        // Assert
        await actual.Should().ThrowAsync<CmdbClientException>();
        messageHandlerMock.Verify();
    }

    [Fact]
    public async Task GetDeploymentMethodAsync_NewMethodParamNull_ShouldThrowException()
    {
        // Arrange
        string? ciName = null;

        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), Mock.Of<HttpMessageHandler>());

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = () => sut.GetDeploymentMethodAsync(ciName);

        // Assert
        await actual.Should().ThrowExactlyAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task InsertDeploymentMethodAsync_CallToHttpClient_ShouldBeSuccessful()
    {
        // Arrange
        var newMethod = _fixture.Build<DeploymentMethod>().FromFactory<string, string, string, string>(
                (organization, project, pipeline, stage) =>
                    new DeploymentMethod(_fixture.Create<string>(), organization, project, pipeline, stage, null))
            .Create();

        var httpMethod = HttpMethod.Post;

        var responseObj = _fixture.Create<ManageDeploymentInformationResponse>();

        var messageHandlerMock = TestHelpers.SetupSuccessHttpMessageHandlerMock<ManageDeploymentInformationRequest>(
            ManageDeploymentInformationEndpoint, httpMethod, content => content.Body!.Key == newMethod.CiName,
            responseObj);

        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), messageHandlerMock.Object);

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = await sut.InsertDeploymentMethodAsync(newMethod);

        // Assert
        messageHandlerMock.Verify();
        actual.Should().BeEquivalentTo(responseObj);
    }

    [Fact]
    public async Task InsertDeploymentMethodAsync_CallToHttpClient_ShouldFail()
    {
        // Arrange
        var newMethod = _fixture.Build<DeploymentMethod>().FromFactory<string, string, string, string>(
                (organization, project, pipeline, stage) =>
                    new DeploymentMethod(_fixture.Create<string>(), organization, project, pipeline, stage, null))
            .Create();

        var httpMethod = HttpMethod.Post;

        var messageHandlerMock =
            TestHelpers.SetupFailHttpMessageHandlerMock(ManageDeploymentInformationEndpoint, httpMethod);
        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), messageHandlerMock.Object);

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = () => sut.InsertDeploymentMethodAsync(newMethod);

        // Assert
        await actual.Should().ThrowAsync<CmdbClientException>();
        messageHandlerMock.Verify();
    }

    [Fact]
    public async Task InsertDeploymentMethodAsync_NewMethodParamNull_ShouldThrowException()
    {
        // Arrange
        DeploymentMethod? newMethod = null;

        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), Mock.Of<HttpMessageHandler>());

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = () => sut.InsertDeploymentMethodAsync(newMethod);

        // Assert
        await actual.Should().ThrowExactlyAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateDeploymentMethodAsync_CallToHttpClient_ShouldBeSuccessful()
    {
        // Arrange
        var configurationItem = _fixture.Create<ConfigurationItem>();
        var newMethod = _fixture.Build<DeploymentMethod>().FromFactory<string, string, string, string>(
                (organization, project, pipeline, stage) =>
                    new DeploymentMethod(_fixture.Create<string>(), organization, project, pipeline, stage, null))
            .Create();
        var currentMethod = SupplementaryInformation.ParseSupplementaryInfo(newMethod.ToString());

        var httpMethod = HttpMethod.Post;

        var responseObj = _fixture.Create<ManageDeploymentInformationResponse>();

        var messageHandlerMock = TestHelpers.SetupSuccessHttpMessageHandlerMock<ManageDeploymentInformationRequest>(
            ManageDeploymentInformationEndpoint, httpMethod, content => content.Body!.Key == configurationItem.CiName,
            responseObj);

        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), messageHandlerMock.Object);

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = await sut.UpdateDeploymentMethodAsync(configurationItem, currentMethod, newMethod);

        // Assert
        messageHandlerMock.Verify();
        actual.Should().BeEquivalentTo(responseObj);
    }

    [Fact]
    public async Task UpdateDeploymentMethodAsync_CallToHttpClient_ShouldFail()
    {
        // Arrange
        var configurationItem = _fixture.Create<ConfigurationItem>();
        var newMethod = _fixture.Build<DeploymentMethod>().FromFactory<string, string, string, string>(
                (organization, project, pipeline, stage) =>
                    new DeploymentMethod(_fixture.Create<string>(), organization, project, pipeline, stage, null))
            .Create();
        var currentMethod = SupplementaryInformation.ParseSupplementaryInfo(newMethod.ToString());

        var httpMethod = HttpMethod.Post;

        var messageHandlerMock =
            TestHelpers.SetupFailHttpMessageHandlerMock(ManageDeploymentInformationEndpoint, httpMethod);
        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), messageHandlerMock.Object);

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = () => sut.UpdateDeploymentMethodAsync(configurationItem, currentMethod, newMethod);

        // Assert
        await actual.Should().ThrowAsync<CmdbClientException>();
        messageHandlerMock.Verify();
    }

    [Fact]
    public async Task UpdateDeploymentMethodAsync_ConfigurationItemParamNull_ShouldThrowException()
    {
        // Arrange
        ConfigurationItem? configurationItem = null;
        var newMethod = _fixture.Build<DeploymentMethod>().FromFactory<string, string, string, string>(
                (organization, project, pipeline, stage) =>
                    new DeploymentMethod(_fixture.Create<string>(), organization, project, pipeline, stage, null))
            .Create();
        var currentMethod = SupplementaryInformation.ParseSupplementaryInfo(newMethod.ToString());

        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), Mock.Of<HttpMessageHandler>());

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = () => sut.UpdateDeploymentMethodAsync(configurationItem, currentMethod, newMethod);

        // Assert
        await actual.Should().ThrowExactlyAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateDeploymentMethodAsync_CurrentMethodParamNull_ShouldThrowException()
    {
        // Arrange
        var configurationItem = _fixture.Create<ConfigurationItem>();
        var newMethod = _fixture.Build<DeploymentMethod>().FromFactory<string, string, string, string>(
                (organization, project, pipeline, stage) =>
                    new DeploymentMethod(_fixture.Create<string>(), organization, project, pipeline, stage, null))
            .Create();
        SupplementaryInformation? currentMethod = null;

        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), Mock.Of<HttpMessageHandler>());

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = () => sut.UpdateDeploymentMethodAsync(configurationItem, currentMethod, newMethod);

        // Assert
        await actual.Should().ThrowExactlyAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateDeploymentMethodAsync_NewMethodParamNull_ShouldThrowException()
    {
        // Arrange
        var configurationItem = _fixture.Create<ConfigurationItem>();
        DeploymentMethod? newMethod = null;
        var currentMethod = SupplementaryInformation.ParseSupplementaryInfo(_fixture.Build<DeploymentMethod>()
            .FromFactory<string, string, string, string>(
                (organization, project, pipeline, stage) =>
                    new DeploymentMethod(_fixture.Create<string>(), organization, project, pipeline, stage, null))
            .Create().ToString());

        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), Mock.Of<HttpMessageHandler>());

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = () => sut.UpdateDeploymentMethodAsync(configurationItem, currentMethod, newMethod);

        // Assert
        await actual.Should().ThrowExactlyAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task DeleteDeploymentMethodAsync_CallToHttpClient_ShouldBeSuccessful()
    {
        // Arrange
        var configurationItem = _fixture.Create<ConfigurationItem>();
        var deploymentMethod = _fixture.Build<DeploymentMethod>().FromFactory<string, string, string, string>(
                (organization, project, pipeline, stage) =>
                    new DeploymentMethod(_fixture.Create<string>(), organization, project, pipeline, stage, null))
            .Create();
        var methodToDelete = SupplementaryInformation.ParseSupplementaryInfo(deploymentMethod.ToString())!;

        var httpMethod = HttpMethod.Post;

        var responseObj = _fixture.Create<ManageDeploymentInformationResponse>();

        var messageHandlerMock = TestHelpers.SetupSuccessHttpMessageHandlerMock<ManageDeploymentInformationRequest>(
            ManageDeploymentInformationEndpoint, httpMethod, content => content.Body!.Key == configurationItem.CiName,
            responseObj);

        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), messageHandlerMock.Object);

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = await sut.DeleteDeploymentMethodAsync(configurationItem, methodToDelete);

        // Assert
        messageHandlerMock.Verify();
        actual.Should().BeEquivalentTo(responseObj);
    }

    [Fact]
    public async Task DeleteDeploymentMethodAsync_CallToHttpClient_ShouldFail()
    {
        // Arrange
        var configurationItem = _fixture.Create<ConfigurationItem>();
        var deploymentMethod = _fixture.Build<DeploymentMethod>().FromFactory<string, string, string, string>(
                (organization, project, pipeline, stage) =>
                    new DeploymentMethod(_fixture.Create<string>(), organization, project, pipeline, stage, null))
            .Create();
        var currentMethod = SupplementaryInformation.ParseSupplementaryInfo(deploymentMethod.ToString())!;

        var httpMethod = HttpMethod.Post;

        var messageHandlerMock =
            TestHelpers.SetupFailHttpMessageHandlerMock(ManageDeploymentInformationEndpoint, httpMethod);
        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), messageHandlerMock.Object);

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = () => sut.DeleteDeploymentMethodAsync(configurationItem, currentMethod);

        // Assert
        await actual.Should().ThrowAsync<CmdbClientException>();
        messageHandlerMock.Verify();
    }

    [Fact]
    public async Task GetAzDoCIsLegacyAsync_CanGetApplicationsAndSubApplications()
    {
        // Arrange
        var httpMethod = HttpMethod.Get;

        var mainCis = new GetCiResponse
        {
            Content = new List<CiContentItem>
            {
                new()
                {
                    Device = new ConfigurationItemModel
                    {
                        CiIdentifier = "MAIN1", AssignmentGroup = "SystemOwner1", Status = "In Use - Production",
                        Environment = new[] { "Production" }
                    }
                },
                new()
                {
                    Device = new ConfigurationItemModel
                    {
                        CiIdentifier = "MAIN2", AssignmentGroup = "SystemOwner2", Status = "In Use - Production",
                        Environment = new[] { "Production" }
                    }
                }
            }
        };

        var subCis = new GetCiResponse
        {
            Content = new List<CiContentItem>
            {
                new()
                {
                    Device = new ConfigurationItemModel
                    {
                        CiIdentifier = "SUB1", Status = "In Use - Production",
                        Environment = new[] { "Production", "Test" }
                    }
                },
                new()
                {
                    Device = new ConfigurationItemModel
                        { CiIdentifier = "SUB2", Status = "In Use - Production", Environment = new[] { "Production" } }
                }
            }
        };

        var messageHandlerMock = TestHelpers.SetupSequenceSuccessHttpMessageHandlerMock(
            "/devices?ConfigurationItemType=Application&DeploymentMethod=Azure Devops&start=1&count=500&view=expand",
            httpMethod, mainCis);
        TestHelpers.SetupSequenceSuccessHttpMessageHandlerMock(
            "/devices?ConfigurationItemType=SubApplication&DeploymentMethod=Azure Devops&start=1&count=500&view=expand",
            httpMethod, subCis, messageHandlerMock);

        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), messageHandlerMock.Object);

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = await sut.GetAzDoCIsLegacyAsync();

        // Assert
        actual.Should().HaveCount(4);
    }

    [Fact]
    public async Task GetAzDoCIsAsync_CanGetApplicationsAndSubApplications()
    {
        // Arrange
        const string Environment = "Production";
        const string Status = "In Use - Production";

        _fixture.Customize<RetrieveCiByQueryResponseInformation>(x => x
            .With(i => i.Environment, new[] { Environment })
            .With(i => i.Status, Status));

        var repsonseInformations = _fixture.CreateMany<RetrieveCiByQueryResponseInformation>(4);
        var retrieveCiByQueryResponse = new RetrieveCiByQueryResponse
        {
            Messages = Array.Empty<string>(),
            RetrieveCiByQuery = new RetrieveCiByQuery
            {
                Information = repsonseInformations
            },
        };

        var messageHandlerMock = TestHelpers.SetupSequenceSuccessHttpMessageHandlerMock(
            "/retrieveCiInfoByQuery",
            HttpMethod.Post, retrieveCiByQueryResponse);

        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), messageHandlerMock.Object);

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = await sut.GetAzDoCIsAsync();

        // Assert
        actual.Should().HaveCount(4);
        var responseInfo = repsonseInformations.First();
        var ci = actual.First().Device!;
        ci.AssignmentGroup.Should().Be(responseInfo.ConfigAdminGroup);
        ci.Status.Should().Be(responseInfo.Status);
        ci.BIVcode.Should().Be(responseInfo.AicClassification);
        ci.SOXClassification.Should().Be(responseInfo.SoxClassification);
        ci.Environment.Should().Contain(responseInfo.Environment);
        ci.CiIdentifier.Should().Be(responseInfo.CiID);
        ci.ConfigurationItem.Should().Be(responseInfo.CiName);
        ci.DisplayName.Should().Be(responseInfo.CiName);
        ci.ConfigurationItemType.Should().Be(responseInfo.CiType);
        ci.ConfigurationItemSubType.Should().Be(responseInfo.CiSubtype);
        var deploymentMethod = responseInfo.DeploymentInfo!.First();
        ci.DeploymentInfo!.First().DeploymentMethod.Should().Be(deploymentMethod.DeploymentMethod);
        ci.DeploymentInfo!.First().SupplementaryInformation.Should().Be(deploymentMethod.SupplementaryInformation);
    }

    [Fact]
    public async Task GetAzDoCIsAsync_TwoBatches_CanGetApplicationsAndSubApplications()
    {
        // Arrange
        const string Environment = "Production";
        const string Status = "In Use - Production";

        _fixture.Customize<RetrieveCiByQueryResponseInformation>(x => x
            .With(i => i.Environment, new[] { Environment })
            .With(i => i.Status, Status));

        var retrieveCiByQueryResponseMoreItems = new RetrieveCiByQueryResponse
        {
            Messages = Array.Empty<string>(),
            RetrieveCiByQuery = new RetrieveCiByQuery
            {
                More = 10,
                Information = _fixture.CreateMany<RetrieveCiByQueryResponseInformation>(100)
            },
        };
        var retrieveCiByQueryResponseLastItems = new RetrieveCiByQueryResponse
        {
            Messages = Array.Empty<string>(),
            RetrieveCiByQuery = new RetrieveCiByQuery
            {
                More = 0,
                Information = _fixture.CreateMany<RetrieveCiByQueryResponseInformation>(10)
            },
        };

        var messageHandlerMock = TestHelpers.SetupSequenceSuccessHttpMessageHandlerMock<RetrieveCiByQueryRequest>(
            "/retrieveCiInfoByQuery",
            HttpMethod.Post, content => content.Body!.StartNum == 101 && content.Body.CountNum == 100,
                retrieveCiByQueryResponseLastItems);

        TestHelpers.SetupSequenceSuccessHttpMessageHandlerMock<RetrieveCiByQueryRequest>(
            "/retrieveCiInfoByQuery",
            HttpMethod.Post, content => content.Body!.StartNum == 1 && content.Body.CountNum == 100,
                retrieveCiByQueryResponseMoreItems, messageHandlerMock);

        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), messageHandlerMock.Object);

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = await sut.GetAzDoCIsAsync();

        // Assert
        actual.Should().HaveCount(110);
    }

    [Theory]
    [InlineData("Planned/On order", new[] { "Production" }, 0)]
    [InlineData("Disposed/Retired", new[] { "Production" }, 0)]
    [InlineData("In Stock - Available", new[] { "Production" }, 0)]
    [InlineData("In Use - Phasing out", new[] { "Production" }, 1)]
    [InlineData("In Use - Pilot", new[] { "Production", "Test" }, 1)]
    [InlineData("In Use - Roll out", new[] { "Test" }, 0)]
    [InlineData("In Use - Test", new[] { "Production" }, 1)]
    [InlineData("In Use - Production", new[] { "Production" }, 1)]
    [InlineData(null, new[] { "Production" }, 0)]
    [InlineData("In Use - Production", null, 0)]
    public async Task GetAzDoCIsAsync_ShouldFilterNonProductionCis(
        string status, string[] environments, int expectedOutcome)
    {
        // Arrange
        var httpMethod = HttpMethod.Get;

        var responseObj = new GetCiResponse
        {
            Content = new List<CiContentItem>
            {
                new()
                {
                    Device = new ConfigurationItemModel
                        { CiIdentifier = "test", Status = status, Environment = environments }
                }
            }
        };

        var messageHandlerMock = TestHelpers.SetupSequenceSuccessHttpMessageHandlerMock(
            "/devices?ConfigurationItemType=Application&DeploymentMethod=Azure Devops&start=1&count=500&view=expand",
            httpMethod, responseObj);
        TestHelpers.SetupSequenceSuccessHttpMessageHandlerMock(
            "/devices?ConfigurationItemType=SubApplication&DeploymentMethod=Azure Devops&start=1&count=500&view=expand",
            httpMethod, null, messageHandlerMock);

        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), messageHandlerMock.Object);

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = await sut.GetAzDoCIsLegacyAsync();

        // Assert
        Assert.Equal(expectedOutcome, actual.Count());
    }

    [Fact]
    public async Task GetAzDoCIsAsync_CallToHttpClient_ShouldBeSuccessful()
    {
        // Arrange
        var httpMethod = HttpMethod.Get;

        var mainCisFirstSet = TestHelpers.CreateCiContentItemResponse(500);
        var mainCisSecondSet = TestHelpers.CreateCiContentItemResponse(500);
        var mainCisLastSet = TestHelpers.CreateCiContentItemResponse(200);

        var subCisFirstSet = TestHelpers.CreateCiContentItemResponse(500);
        var subCisSecondSet = TestHelpers.CreateCiContentItemResponse(400);

        var messageHandlerMock = TestHelpers.SetupSequenceSuccessHttpMessageHandlerMock(
            "/devices?ConfigurationItemType=Application&DeploymentMethod=Azure Devops&start=1&count=500&view=expand",
            httpMethod, mainCisFirstSet);
        TestHelpers.SetupSequenceSuccessHttpMessageHandlerMock(
            "/devices?ConfigurationItemType=Application&DeploymentMethod=Azure Devops&start=501&count=500&view=expand",
            httpMethod, mainCisSecondSet, messageHandlerMock);
        TestHelpers.SetupSequenceSuccessHttpMessageHandlerMock(
            "/devices?ConfigurationItemType=Application&DeploymentMethod=Azure Devops&start=1001&count=500&view=expand",
            httpMethod, mainCisLastSet, messageHandlerMock);
        TestHelpers.SetupSequenceSuccessHttpMessageHandlerMock(
            "/devices?ConfigurationItemType=SubApplication&DeploymentMethod=Azure Devops&start=1&count=500&view=expand",
            httpMethod, subCisFirstSet, messageHandlerMock);
        TestHelpers.SetupSequenceSuccessHttpMessageHandlerMock(
            "/devices?ConfigurationItemType=SubApplication&DeploymentMethod=Azure Devops&start=501&count=500&view=expand",
            httpMethod, subCisSecondSet, messageHandlerMock);

        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(CmdbClient), messageHandlerMock.Object);

        var sut = new CmdbClient(httpClientFactoryMock.Object);

        // Act
        var actual = await sut.GetAzDoCIsLegacyAsync();

        // Assert
        actual.Should().HaveCount(2100);
    }
}