#nullable enable

using FluentAssertions;
using System.Net.Http;
using Rabobank.Compliancy.Domain.Extensions;
using Xunit;

namespace Rabobank.Compliancy.Core.InputValidation.Tests.Extensions;

public class InputValidationExtensionsTests
{
    [Fact]
    public void StripSensitiveInformationFromHeader_RequestNull_ReturnsNull()
    {
        // Arrange
        HttpRequestMessage? request = null;

        // Act
        var result = request.StripSensitiveInformationFromHeader();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void StripSensitiveInformationFromHeader_RequestWithAuthorizationHeader_ReturnsRequestWithoutAuthorizationHeader()
    {
        // Arrange
        HttpRequestMessage request = new();
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "Secret code");

        // Act
        var result = request.StripSensitiveInformationFromHeader();

        // Assert
        result!.Headers.Authorization.Should().BeNull();
    }

    [Fact]
    public void StripSensitiveInformationFromHeader_RequestSensitiveAndNonSensitiveHeaders_ReturnsRequestWithoutSensitiveHeaders()
    {
        // Arrange
        const string insensitiveHeader1 = "Dummy";
        const string insensitiveHeader2 = "Dummy2";
        const string sensitiveHeader = "x-functions-key";
        HttpRequestMessage request = new();
        request.Headers.Add(insensitiveHeader1, "test");
        request.Headers.Add(insensitiveHeader2, "test2");
        request.Headers.Add(sensitiveHeader, "functioncode");

        // Act
        var result = request.StripSensitiveInformationFromHeader();

        // Assert
        result!.Headers.Should().Contain(k => k.Key == insensitiveHeader1);
        result.Headers.Should().Contain(k => k.Key == insensitiveHeader2);
        result.Headers.Should().NotContain(k => k.Key == sensitiveHeader);
    }
}