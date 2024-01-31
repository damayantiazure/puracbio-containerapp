using FluentAssertions;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Evaluatables;
using Rabobank.Compliancy.Domain.Compliancy.Rules;

namespace Rabobank.Compliancy.Domain.Tests;

/// <summary>
/// These tests are implemented against the ClassicReleasePipelineHasRequiredRetentionPolicy rule
/// but do also apply for the rule YamlReleasePipelineHasRequiredRetentionPolicy since they both
/// inheret from same base class both both rules do not have a additional implementation yet    
/// </summary>
public class ClassicReleasePipelineHasRequiredRetentionPolicyTests
{
    private readonly ClassicReleasePipelineHasRequiredRetentionPolicy _sut;

    public ClassicReleasePipelineHasRequiredRetentionPolicyTests()
    {
        _sut = new ClassicReleasePipelineHasRequiredRetentionPolicy();
    }

    [Fact]
    public void Evaluate_ClassicReleasePipelineHasRequiredRetentionPolicyNoRetentionSettings_ReturnsFalseResult()
    {
        // Arrange
        var pipeline = new Pipeline
        {
            Settings = new List<ISettings> { null }
        };

        var settingsEvaluatable = new SettingsEvaluatable(pipeline);

        // Act
        var result = _sut.Evaluate(settingsEvaluatable);

        // Assert
        result.Passed.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_ClassicReleasePipelineHasRequiredRetentionPolicy500Days_ReturnsTrueResult()
    {
        // Arrange
        var pipeline = new Pipeline
        {
            Settings = new List<ISettings> { new RetentionSettings { DaysToKeepRuns = 500 } }
        };
        var settingsEvaluatable = new SettingsEvaluatable(pipeline);

        // Act
        var result = _sut.Evaluate(settingsEvaluatable);

        // Assert
        result.Passed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_ClassicReleasePipelineHasRequiredRetentionPolicy450Days_ReturnsTrueResult()
    {
        // Arrange
        var pipeline = new Pipeline
        {
            Settings = new List<ISettings> { new RetentionSettings { DaysToKeepRuns = 450 } }
        };
        var settingsEvaluatable = new SettingsEvaluatable(pipeline);

        // Act
        var result = _sut.Evaluate(settingsEvaluatable);

        // Assert
        result.Passed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_ClassicReleasePipelineHasRequiredRetentionPolicy449Days_ReturnsFalseResult()
    {
        // Arrange
        var pipeline = new Pipeline
        {
            Settings = new List<ISettings> { new RetentionSettings { DaysToKeepRuns = 449 } }
        };
        var projectSettingsEvaluatable = new SettingsEvaluatable(pipeline);

        // Act
        var result = _sut.Evaluate(projectSettingsEvaluatable);

        // Assert
        result.Passed.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_ClassicReleasePipelineHasRequiredRetentionPolicy100Days_ReturnsFalseResult()
    {
        // Arrange
        var pipeline = new Pipeline
        {
            Settings = new List<ISettings> { new RetentionSettings { DaysToKeepRuns = 100 } }
        };
        var projectSettingsEvaluatable = new SettingsEvaluatable(pipeline);

        // Act
        var result = _sut.Evaluate(projectSettingsEvaluatable);

        // Assert
        result.Passed.Should().BeFalse();
    }
}