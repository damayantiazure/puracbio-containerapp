using Rabobank.Compliancy.Domain.RuleProfiles;

namespace Rabobank.Compliancy.Domain.Tests;

public class RuleProfileTests
{
    [Theory]
    [InlineData(nameof(Profiles.Default), typeof(DefaultRuleProfile), "Default")]
    [InlineData(nameof(Profiles.MainframeCobol), typeof(MainFrameCobolRuleProfile), "MainframeCobol")]
    public void RuleProfile_GetRuleProfile_ReturnsCorrectProfile_WithCorrectName(string ruleProfileName, Type profileType, string actualProfileName)
    {
        // Arrange and Act
        var ruleProfile = RuleProfile.GetProfile(ruleProfileName);

        // Assert
        Assert.Equal(profileType, ruleProfile.GetType());
        Assert.Equal(actualProfileName, ruleProfile.Name);
    }
}