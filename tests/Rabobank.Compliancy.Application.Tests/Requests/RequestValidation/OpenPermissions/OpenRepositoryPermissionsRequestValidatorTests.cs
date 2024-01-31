using FluentValidation.TestHelper;
using Rabobank.Compliancy.Application.Requests.OpenPermissions;
using Rabobank.Compliancy.Application.Requests.RequestValidation.OpenPermissions;

namespace Rabobank.Compliancy.Application.Tests.Requests.RequestValidation.OpenPermissions;

public class OpenRepositoryPermissionsRequestValidatorTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly OpenGitRepoPermissionsRequestValidator _sut;

    public OpenRepositoryPermissionsRequestValidatorTests()
    {
        _sut = new OpenGitRepoPermissionsRequestValidator();
    }

    [Fact]
    public async Task TestValidateAsync_WhenGitRepoIdIsEmpty_ShouldHaveValidationErrorForGitRepoId()
    {
        // Arrange
        var openGitRepoPermissionsRequest = _fixture.Build<OpenGitRepoPermissionsRequest>()
            .Without(x => x.GitRepoId).Create();

        // Act
        var actual = await _sut.TestValidateAsync(openGitRepoPermissionsRequest);

        // Assert
        actual.ShouldHaveValidationErrorFor(x => x.GitRepoId);
    }

    [Fact]
    public async Task TestValidateAsync_WhenGitRepoIdHasValidValue_ShouldNotHaveValidationErrorForGitRepoId()
    {
        // Arrange
        var openGitRepoPermissionsRequest = _fixture.Create<OpenGitRepoPermissionsRequest>();

        // Act
        var actual = await _sut.TestValidateAsync(openGitRepoPermissionsRequest);

        // Assert
        actual.ShouldNotHaveValidationErrorFor(x => x.GitRepoId);
    }
}