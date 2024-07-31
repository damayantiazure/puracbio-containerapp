#nullable enable

#pragma warning disable CS0618 // The team assessed the risk of using these interfaces and decided 
// it's better than running isolated processes (which is the alternative and supports middleware).

// This has been in preview for years and Microsoft utters the intention of removing the obsolete
// attribute rather than removing the functionality. Please refer to the discussion on
// https://github.com/Azure/azure-webjobs-sdk/issues/2546 for more information.

using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Requests.RequestValidation;
using Rabobank.Compliancy.Application.Security;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Validation;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Tests;

public class BaseFunctionTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<ILoggingService> _loggingServiceMock = new();
    private readonly Mock<ISecurityContext> _securityContextMock = new();
    private readonly BaseFunction _sut;

    public BaseFunctionTests()
    {
        var registerDeviationProcessMock = new Mock<IRegisterDeviationProcess>();
        _sut = new RegisterDeviationFunction(registerDeviationProcessMock.Object, _loggingServiceMock.Object,
            _httpContextAccessorMock.Object, _securityContextMock.Object);
    }

    [Fact]
    public void OnExecutingAsync_NonValidatableArgument_Should_CompleteTask()
    {
        // Arrange
        var arguments = new Dictionary<string, object>
        {
            {
                _fixture.Create<string>(), _fixture.Create<object>()
            }
        };
        var properties = new Dictionary<string, object>();
        var functionInstanceId = _fixture.Create<Guid>();
        var functionName = _fixture.Create<string>();
        var logger = new Mock<ILogger>();

        var context =
            new FunctionExecutingContext(arguments, properties, functionInstanceId, functionName, logger.Object);

        // Act
        var result = _sut.OnExecutingAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBe(Task.CompletedTask);
    }

    [Fact]
    public void OnExecutingAsync_ValidatableArgument_When_Valid_Should_CompleteTask()
    {
        // Arrange
        var validatableMock = new Mock<IValidatable>();
        var validationResult = new ValidationResult();
        validatableMock.Setup(x => x.Validate()).Returns(validationResult);
        var arguments = new Dictionary<string, object>
        {
            {
                _fixture.Create<string>(), validatableMock.Object
            }
        };
        var properties = new Dictionary<string, object>();
        var functionInstanceId = _fixture.Create<Guid>();
        var functionName = _fixture.Create<string>();
        var logger = new Mock<ILogger>();

        var context =
            new FunctionExecutingContext(arguments, properties, functionInstanceId, functionName, logger.Object);

        // Act
        var result = _sut.OnExecutingAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBe(Task.CompletedTask);
    }

    [Fact]
    public void OnExecutingAsync_ValidatableArgument_When_Invalid_Should_Throw_ValidationErrorsException()
    {
        // Arrange
        var validatableMock = new Mock<IValidatable>();
        var validationResult = new ValidationResult
        {
            Errors = new List<ValidationFailure>
            {
                new("property", "message")
            }
        };
        validatableMock.Setup(x => x.Validate()).Returns(validationResult);
        var arguments = new Dictionary<string, object>
        {
            {
                _fixture.Create<string>(), validatableMock.Object
            }
        };
        var properties = new Dictionary<string, object>();
        var functionInstanceId = _fixture.Create<Guid>();
        var functionName = _fixture.Create<string>();
        var logger = new Mock<ILogger>();

        var context =
            new FunctionExecutingContext(arguments, properties, functionInstanceId, functionName, logger.Object);

        // Act & Assert
        Assert.ThrowsAsync<ValidationErrorsException>(() =>
            _sut.OnExecutingAsync(context, CancellationToken.None));
    }
}
#pragma warning restore CS0618 // Type or member is obsolete