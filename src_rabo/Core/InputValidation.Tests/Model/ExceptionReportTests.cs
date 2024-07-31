#nullable enable

using AutoFixture;
using FluentAssertions;
using Flurl.Http;
using Rabobank.Compliancy.Core.InputValidation.Model;
using System;
using Xunit;

namespace Rabobank.Compliancy.Core.InputValidation.Tests.Model;

public class ExceptionReportTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void InitializeConstructor_WithRunIdValueAsNull_ShouldNotThrowException()
    {
        // Arrange
        var exceptionBaseMetaInformation = _fixture.Build<ExceptionBaseMetaInformation>()
            .Without(x => x.RunId).Create();

        // Act
        var actual = () => new ExceptionReport(exceptionBaseMetaInformation);

        // Assert
        actual.Should().NotThrow();
    }

    [Fact]
    public void InitializeConstructor_WithExceptionAsParameter_ShouldSetExceptionInformation()
    {
        // Arrange
        var exception = _fixture.Create<Exception>();

        // Act
        var actual = new ExceptionReport(exception);

        // Assert
        actual.ExceptionType.Should().Be(exception.GetType().Name);
        actual.ExceptionMessage.Should().Be($"{exception.Message} Stacktrace: {exception.StackTrace}");
        actual.InnerExceptionType.Should().Be(exception.InnerException?.GetType().Name);
        actual.InnerExceptionMessage.Should().Be(exception.InnerException?.Message);
    }

    [Fact]
    public void InitializeConstructor_WithExceptionBaseMetaInformationAsParameter_ShouldSetExceptionInformation()
    {
        // Arrange
        var exceptionBaseMetaInformation = _fixture.Create<ExceptionBaseMetaInformation>();

        // Act
        var actual = new ExceptionReport(exceptionBaseMetaInformation);

        // Assert
        actual.FunctionName.Should().Be(exceptionBaseMetaInformation.Function);
        actual.RequestUrl.Should().Be(exceptionBaseMetaInformation.RequestUrl);
        actual.Organization.Should().Be(exceptionBaseMetaInformation.Organization);
        actual.ProjectId.Should().Be(exceptionBaseMetaInformation.ProjectId);
        actual.RunId.Should().Be(exceptionBaseMetaInformation.RunId);
        actual.ReleaseId.Should().Be(exceptionBaseMetaInformation.ReleaseId);
        actual.ReleaseUrl.Should().Be(exceptionBaseMetaInformation.ReleaseUrl);
        actual.RequestData.Should().Be(exceptionBaseMetaInformation.RequestData);
        actual.PipelineType.Should().Be(exceptionBaseMetaInformation.PipelineType);
        actual.PullRequestUrl.Should().Be(exceptionBaseMetaInformation.PullRequestUrl);
        actual.RunUrl.Should().Be(exceptionBaseMetaInformation.RunUrl);
        actual.Request.Should().Be(exceptionBaseMetaInformation.Request);
    }

    [Fact]
    public void InitializeConstructor_WithNullExceptionAsParameter_ShouldNotSetValues()
    {
        // Arrange
        Exception? exception = null;

        // Act
        var actual = new ExceptionReport(exception);

        // Assert
        actual.ExceptionType.Should().BeNull();
        actual.ExceptionMessage.Should().BeNull();
        actual.InnerExceptionType.Should().BeNull();
        actual.InnerExceptionMessage.Should().BeNull();
    }

    [Fact]
    public void InitializeConstructor_WithNullInnerException_ShouldNotSetValuesForInnerException()
    {
        // Arrange
        Exception? inner = null;
        const string message = "this is my exception message";
        var exception = new Exception(message, inner);

        // Act
        var actual = new ExceptionReport(exception);

        // Assert
        actual.ExceptionType.Should().NotBeEmpty();
        actual.ExceptionMessage.Should().NotBeEmpty();
        actual.InnerExceptionType.Should().BeNull();
        actual.InnerExceptionMessage.Should().BeNull();
    }

    [Fact]
    public void InitializeConstructor_WithFlurlHttpExceptionAsNull_ShouldNotThrowException()
    {
        // Arrange
        FlurlHttpException? exception = null;

        // Act
        var actual = () => new ExceptionReport(exception);

        // Assert
        actual.Should().NotThrow();
    }

    [Fact]
    public void InitializeConstructor_WithExceptionAsNull_ShouldNotThrowException()
    {
        // Arrange
        Exception? exception = null;

        // Act
        var actual = () => new ExceptionReport(exception);

        // Assert
        actual.Should().NotThrow();
    }
}