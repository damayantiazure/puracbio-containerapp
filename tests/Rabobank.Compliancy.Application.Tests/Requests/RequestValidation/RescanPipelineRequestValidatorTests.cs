using FluentValidation.TestHelper;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Requests.RequestValidation;

namespace Rabobank.Compliancy.Application.Tests.Requests.RequestValidation;

public class RescanPipelineRequestValidatorTests
{
    private readonly RescanPipelineRequestValidator _sut;
    private readonly IFixture _fixture = new Fixture();

    public RescanPipelineRequestValidatorTests()
    {
        _sut = new RescanPipelineRequestValidator();
    }

    [Fact]
    public async Task TestValidateAsync_WithNullOrEmptyPipelineId_ShouldHaveValidationError()
    {
        // Arrange
        var rescanPipelineRequest = _fixture.Build<RescanPipelineRequest>()
            .Without(x => x.PipelineId).Create();

        // Act
        var actual = await _sut.TestValidateAsync(rescanPipelineRequest);

        // Assert
        actual.ShouldHaveValidationErrorFor(x => x.PipelineId);
    }

    [Fact]
    public async Task TestValidateAsync_WithEmptyOrganization_ShouldHaveValidationError()
    {
        // Arrange
        var rescanPipelineRequest = _fixture.Build<RescanPipelineRequest>()
            .Without(x => x.Organization).Create();

        // Act
        var actual = await _sut.TestValidateAsync(rescanPipelineRequest);

        // Assert
        actual.ShouldHaveValidationErrorFor(x => x.Organization);
    }

    [Fact]
    public async Task TestValidateAsync_WithEmptyProjectId_ShouldHaveValidationError()
    {
        // Arrange
        var rescanPipelineRequest = _fixture.Build<RescanPipelineRequest>()
            .With(x => x.ProjectId, Guid.Empty).Create();

        // Act
        var actual = await _sut.TestValidateAsync(rescanPipelineRequest);

        // Assert
        actual.ShouldHaveValidationErrorFor(x => x.ProjectId);
    }

    [Fact]
    public async Task TestValidateAsync_WithCorrectlyFilledProperties_ShouldNotHaveValidationError()
    {
        // Arrange
        var rescanPipelineRequest = _fixture.Create<RescanPipelineRequest>();

        // Act
        var actual = await _sut.TestValidateAsync(rescanPipelineRequest);

        // Assert
        actual.ShouldNotHaveAnyValidationErrors();
    }
}