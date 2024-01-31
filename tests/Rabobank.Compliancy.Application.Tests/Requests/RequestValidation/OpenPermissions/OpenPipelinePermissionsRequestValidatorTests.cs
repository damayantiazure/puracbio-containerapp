using FluentAssertions;
using FluentValidation.TestHelper;
using Rabobank.Compliancy.Application.Requests.OpenPermissions;
using Rabobank.Compliancy.Application.Requests.RequestValidation.OpenPermissions;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Application.Tests.Requests.RequestValidation.OpenPermissions;

public class OpenPipelinePermissionsRequestValidatorTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly OpenPipelinePermissionsRequestValidator<Pipeline> _sut;

    public OpenPipelinePermissionsRequestValidatorTests()
    {
        _sut = new OpenPipelinePermissionsRequestValidator<Pipeline>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task TestValidateAsync_WhenPipelineIdIsZeroOrNegativeValue_ShouldHaveValidationErrorForPipelineId(int pipelineId)
    {
        // Arrange
        var openpermissionPermissionsRequest = _fixture.Build<OpenPipelinePermissionsRequest<Pipeline>>()
            .With(x => x.PipelineId, pipelineId).Create();

        // Act
        var actual = await _sut.TestValidateAsync(openpermissionPermissionsRequest);

        // Assert
        actual.ShouldHaveValidationErrorFor(x => x.PipelineId);
    }

    [Fact]
    public async Task TestValidateAsync_WhenPipelineIdHasNegativeValue_ShouldHaveErrorMessage()
    {
        // Arrange
        var openpermissionPermissionsRequest = _fixture.Build<OpenPipelinePermissionsRequest<Pipeline>>()
            .With(x => x.PipelineId, -1).Create();

        // Act
        var actual = await _sut.TestValidateAsync(openpermissionPermissionsRequest);

        // Assert
        actual.Errors[0].ErrorMessage.Should().Be("'PipelineId' must be greater than zero.");
    }

    [Fact]
    public async Task TestValidateAsync_WhenPipelineIdHasPositiveValue_ShouldNotHaveValidationErrorForPipelineId()
    {
        // Arrange
        var openpermissionPermissionsRequest = _fixture.Create<OpenPipelinePermissionsRequest<Pipeline>>();

        // Act
        var actual = await _sut.TestValidateAsync(openpermissionPermissionsRequest);

        // Assert
        actual.ShouldNotHaveValidationErrorFor(x => x.PipelineId);
    }
}