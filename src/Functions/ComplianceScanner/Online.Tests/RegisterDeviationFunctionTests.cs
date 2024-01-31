#nullable enable

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Security;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.Shared.Tests;
using Shouldly;
using System;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Tests;

public class RegisterDeviationFunctionTests : FunctionRequestTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<IRegisterDeviationProcess> _registerDeviationProcessMock = new();
    private readonly RegisterDeviationFunction _sut;
    private readonly Mock<ILoggingService> _loggingServiceMock = new();
    private readonly Mock<ISecurityContext> _securityContextMock = new();

    public RegisterDeviationFunctionTests() =>
        _sut = new RegisterDeviationFunction(_registerDeviationProcessMock.Object,
            _loggingServiceMock.Object, _httpContextAccessorMock.Object, _securityContextMock.Object);

    [Fact]
    public async Task Unauthorized_ShouldReturn_UnauthorizedResult()
    {
        // Arrange
        var registerDeviationRequest = _fixture.Create<RegisterDeviationRequest>();
        var httpRequestMock = new Mock<HttpRequest>();
        httpRequestMock
            .Setup(_ => _.Headers)
            .Returns(_fixture.Create<HeaderDictionary>());

        // Act
        var actual =
            await _sut.RegisterDeviation(registerDeviationRequest, httpRequestMock.Object, CancellationToken.None);

        // Assert
        Assert.IsType<UnauthorizedResult>(actual);
    }

    [Fact]
    public async Task ValidInput_ShouldReturn_OkResult()
    {
        // Arrange
        var registerDeviationRequest = _fixture.Create<RegisterDeviationRequest>();
        var httpRequestMock = new Mock<HttpRequest>();
        httpRequestMock
            .Setup(_ => _.Headers.Authorization)
            .Returns(_fixture.Create<string>());

        // Act
        var actual =
            await _sut.RegisterDeviation(registerDeviationRequest, httpRequestMock.Object, CancellationToken.None);

        // Assert
        Assert.IsType<OkResult>(actual);
    }

    [Fact]
    public async Task ValidInput_Should_Register_Deviation()
    {
        // Arrange
        var registerDeviationRequest = _fixture.Create<RegisterDeviationRequest>();
        var httpRequestMock = new Mock<HttpRequest>();
        var authorization = new StringValues(_fixture.Create<string>());
        var authenticationHeader = AuthenticationHeaderValue.Parse(authorization);

        httpRequestMock
            .Setup(_ => _.Headers.Authorization)
            .Returns(authorization);

        // Act
        await _sut.RegisterDeviation(registerDeviationRequest, httpRequestMock.Object, CancellationToken.None);

        // Assert
        _registerDeviationProcessMock.Verify(_ =>
            _.RegisterDeviation(registerDeviationRequest, authenticationHeader, CancellationToken.None));
    }

    [Fact]
    public async Task RegisterDeviation_WithUnexpectedException_ShouldThrowException()
    {
        // Arrange
        var registerDeviationRequest = _fixture.Create<RegisterDeviationRequest>();
        var httpRequestMock = new Mock<HttpRequest>();
        var authorization = new StringValues(_fixture.Create<string>());
        var authenticationHeader = AuthenticationHeaderValue.Parse(authorization);

        httpRequestMock
            .Setup(_ => _.Headers.Authorization)
            .Returns(authorization);

        _registerDeviationProcessMock.Setup(m =>
                m.RegisterDeviation(registerDeviationRequest, authenticationHeader, CancellationToken.None))
            .Throws<InvalidOperationException>();

        // Act
        var actual = () =>
            _sut.RegisterDeviation(registerDeviationRequest, httpRequestMock.Object, CancellationToken.None);

        // Assert
        await actual.Should().ThrowAsync<Exception>();
        _loggingServiceMock.Verify(item => item.LogExceptionAsync(
            LogDestinations.ComplianceScannerOnlineErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<InvalidOperationException>()), Times.Once);
    }
}