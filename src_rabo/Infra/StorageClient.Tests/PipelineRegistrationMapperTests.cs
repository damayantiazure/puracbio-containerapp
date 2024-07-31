using AutoFixture;
using Newtonsoft.Json;
using Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Rabobank.Compliancy.Infra.StorageClient.Tests;

public class PipelineRegistrationMapperTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly PipelineRegistrationMapper _sut;

    public PipelineRegistrationMapperTests()
    {
        _sut = new PipelineRegistrationMapper();
    }

    [Fact]
    public void CanMapDeploymentMethods()
    {
        // Arrange
        var organization = "raboweb";
        var ciIdentifier = "123";
        var pipelineId = "123";
        var projectId = "456";
        var stageId = "789";
        var isSoxApplicationValue = "Critical";
        var aicRating = "222";
        var ciSubtype = "Automation Software";

        var deploymentInfo = new DeploymentInfo
        {
            DeploymentMethod = "Azure DevOps",
            SupplementaryInformation = $"{{\"organization\":\"{organization}\",\"project\":\"{projectId}\",\"pipeline\":\"{pipelineId}\",\"stage\":\"{stageId}\"}}"
        };

        var ciContentItem = new CiContentItem
        {
            Device = new ConfigurationItemModel
            {
                CiIdentifier = ciIdentifier,
                DisplayName = "CiName",
                AssignmentGroup = "SystemOwner",
                SOXClassification = isSoxApplicationValue,
                DeploymentInfo = new[] { deploymentInfo },
                BIVcode = aicRating,
                ConfigurationItemSubType = ciSubtype
            }
        };

        // Act
        var actual = _sut.Map(ciContentItem).FirstOrDefault();

        // Assert
        Assert.Equal(organization, actual.Organization);
        Assert.Equal(ciIdentifier, actual.CiIdentifier);
        Assert.Equal(pipelineId, actual.PipelineId);
        Assert.Equal(projectId, actual.ProjectId);
        Assert.Equal(stageId, actual.StageId);
        Assert.True(actual.IsSoxApplication);
        Assert.Equal(aicRating, actual.AicRating);
        Assert.Equal(ciSubtype, actual.CiSubtype);
    }

    [Fact]
    public void ShouldSkipInvalidDeploymentMethods()
    {
        // Arrange
        var deploymentInfo = new DeploymentInfo { SupplementaryInformation = "Invalid JSON" };
        var ciContentItem = new CiContentItem
        {
            Device = new ConfigurationItemModel
            {
                CiIdentifier = "123",
                DisplayName = "CiName",
                AssignmentGroup = "SystemOwner",
                DeploymentInfo = new List<DeploymentInfo> { deploymentInfo }
            }
        };

        // Act
        var actual = _sut.Map(ciContentItem);
        Assert.Empty(actual);
    }

    [Fact]
    public void Map_WithNoDeploymentInfo_ShouldReturnEmptyCollection()
    {
        // Arrange
        var ciContentItem = _fixture.Create<CiContentItem>();

        // Act
        var actual = _sut.Map(ciContentItem);

        // Assert
        actual.ShouldBeEmpty();
    }

    [Fact]
    public void MapToConfigurationItem_WhenDeviceIsNull_ShouldReturnNull()
    {
        // Arrange
        var ciContentItem = _fixture.Build<CiContentItem>().Without(x => x.Device).Create();

        // Act
        var actual = PipelineRegistrationMapper.MapToConfigurationItem(ciContentItem);

        // Assert
        actual.ShouldBeNull();
    }

    [Fact]
    public void MapToConfigurationItem_WhenDeviceIsNotNull_ShouldReturnDeviceObject()
    {
        // Arrange
        var ciContentItem = _fixture.Create<CiContentItem>();
        var expected = ciContentItem.Device;

        // Act
        var actual = PipelineRegistrationMapper.MapToConfigurationItem(ciContentItem);

        // Assert
        actual.AicClassification.ShouldBe(expected.BIVcode);
        actual.CiID.ShouldBe(expected.CiIdentifier);
        actual.CiName.ShouldBe(expected.DisplayName);
        actual.CiType.ShouldBe(expected.ConfigurationItemType);
        actual.CiSubtype.ShouldBe(expected.ConfigurationItemSubType);
        actual.ConfigAdminGroup.ShouldBe(expected.AssignmentGroup);
        actual.Environment.ShouldBe(expected.Environment);
        actual.SOXClassification.ShouldBe(expected.SOXClassification);
        actual.Status.ShouldBe(expected.Status);
    }
}