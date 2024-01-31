using AutoFixture;
using Rabobank.Compliancy.Domain.RuleProfiles;
using Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using Shouldly;
using System;
using Xunit;

namespace Rabobank.Compliancy.Infra.StorageClient.Tests.Model;

public class PipelineRegistrationTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void PipelineRegistration_ConstructedWithNullCiContentItem_AndNotNullStage_IsNonProd()
    {
        //Arrange
        const string stageId = "MyAwesomeStage";
        var projectId = _fixture.Create<string>();
        var organization = _fixture.Create<string>();
        var pipelineId = _fixture.Create<string>();
        var profile = _fixture.Create<string>();


        //Act
        var pipelineRegistration = new PipelineRegistration(null, organization, projectId, pipelineId, null, stageId, profile);

        //Assert
        Assert.False(pipelineRegistration.IsProduction);
        Assert.Equal(PipelineRegistration.NonProd, pipelineRegistration.PartitionKey);
    }

    [Fact]
    public void PipelineRegistration_ConstructedWithCiContentItem_WithNotNullCiIdentifier_AndNotNullStage_IsProd()
    {
        //Arrange
        const string stageId = "MyAwesomeStage";
        var projectId = _fixture.Create<string>();
        var organization = _fixture.Create<string>();
        var pipelineId = _fixture.Create<string>();
        var profile = _fixture.Create<string>();
        var ciContentItem = new ConfigurationItem()
        {
            CiID = "CI0009001"
        };

        //Act
        var pipelineRegistration = new PipelineRegistration(ciContentItem, organization, projectId, pipelineId, null, stageId, profile);

        //Assert
        Assert.True(pipelineRegistration.IsProduction);
        Assert.Equal(PipelineRegistration.Prod, pipelineRegistration.PartitionKey);
    }

    [Theory]
    [InlineData(nameof(Profiles.Default), typeof(DefaultRuleProfile))]
    [InlineData(nameof(Profiles.MainframeCobol), typeof(MainFrameCobolRuleProfile))]
    public void PipelineRegistration_GetRuleProfile_ReturnsCorrectProfileType(string profileName, Type profileType)
    {
        // Arrange
        var registration = GetPipelineRegistrationWithProfileName(profileName);

        // Act
        var registrationProfile = registration.GetRuleProfile();

        // Assert
        Assert.IsType(profileType, registrationProfile);
    }

    [Theory]
    [InlineData("defaultProfile")]
    [InlineData("somethingElse")]
    [InlineData("default")]
    [InlineData(nameof(Profiles.Default))]
    public void NotvalidProfileName_ShouldReturn_DefaultProfile(string profileName)
    {
        // Arrange
        var registration = GetPipelineRegistrationWithProfileName(profileName);

        // Act
        var profile = registration.GetRuleProfile();

        // Assert
        Assert.IsType<DefaultRuleProfile>(profile);
    }

    [Fact]
    public void PipelineRegistration_WhenConstructorInitialized_ShouldHaveDefaultRuleProfileName()
    {
        // Arrange
        var pipelineRegistration = new PipelineRegistration();

        // Act
        var actual = pipelineRegistration.RuleProfileName;

        // Assert
        actual.ShouldBe(nameof(Profiles.Default));
    }

    [Fact]
    public void PipelineRegistration_WithMainFrameRuleProfileName_ShouldReturnMainFrameRuleProfileName()
    {
        // Arrange
        var pipelineRegistration = new PipelineRegistration
        {
            RuleProfileName = nameof(Profiles.MainframeCobol)
        };

        // Act
        var actual = pipelineRegistration.RuleProfileName;

        // Assert
        actual.ShouldBe(nameof(Profiles.MainframeCobol));
    }

    [Fact]
    public void PipelineRegistration_WithInvalidRuleProfileName_ShouldHaveDefaultRuleProfileName()
    {
        // Arrange
        var pipelineRegistration = new PipelineRegistration
        {
            RuleProfileName = _fixture.Create<string>()
        };

        // Act
        var actual = pipelineRegistration.RuleProfileName;

        // Assert
        actual.ShouldBe(nameof(Profiles.Default));
    }

    private static PipelineRegistration GetPipelineRegistrationWithProfileName(string profileName)
    {
        var pipelineRegistration = new PipelineRegistration()
        {
            RuleProfileName = profileName
        };

        return pipelineRegistration;
    }
}