using FluentValidation.TestHelper;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Requests.RequestValidation;
using Rabobank.Compliancy.Domain.Rules;

namespace Rabobank.Compliancy.Application.Tests.Requests.RequestValidation;

public class DeleteDeviationRequestValidatorTest
{
    private readonly IFixture _fixture = new Fixture();

    private const string EmptyString = "";
    private const string WhiteSpaceString = " ";
    private const string InvalidGuid = "00000000-0000-0000-0000-000000000000";
    private const string ValidCiIdentifier = "CI1234567";
    private readonly DeleteDeviationRequestValidator _sut;

    public DeleteDeviationRequestValidatorTest()
    {
        _sut = new DeleteDeviationRequestValidator();
    }

    [Theory]
    [InlineData(EmptyString)]
    [InlineData(WhiteSpaceString)]
    [InlineData(null)]
    public async Task TestValidateAsync_WithInvalidOrganization_ShouldHaveValidationError(string organization)
    {
        // Arrange
        var deleteDeviationRequest = _fixture.Build<DeleteDeviationRequest>()
            .With(x => x.Organization, organization).Create();

        // Act
        var actual = await _sut.TestValidateAsync(deleteDeviationRequest);

        // Assert
        actual.ShouldHaveValidationErrorFor(x => x.Organization);
    }

    [Theory]
    [InlineData(EmptyString)]
    [InlineData(WhiteSpaceString)]
    [InlineData("-1")]
    [InlineData(InvalidGuid)]
    [InlineData(null)]
    public async Task TestValidateAsync_WithInvalidItemId_ShouldHaveValidationError(string itemId)
    {
        // Arrange
        var deleteDeviationRequest = _fixture.Build<DeleteDeviationRequest>()
            .With(x => x.ItemId, itemId).Create();

        // Act
        var actual = await _sut.TestValidateAsync(deleteDeviationRequest);

        // Assert
        actual.ShouldHaveValidationErrorFor(x => x.ItemId);
    }

    [Fact]
    public async Task TestValidateAsync_WithInvalidProjectId_ShouldHaveValidationError()
    {
        // Arrange
        var deleteDeviationRequest = _fixture.Build<DeleteDeviationRequest>()
            .With(x => x.ProjectId, Guid.Parse(InvalidGuid)).Create();

        // Act
        var actual = await _sut.TestValidateAsync(deleteDeviationRequest);

        // Assert
        actual.ShouldHaveValidationErrorFor(x => x.ProjectId);
    }

    [Theory]
    [InlineData(EmptyString)]
    [InlineData(WhiteSpaceString)]
    [InlineData("qwety")]
    [InlineData(null)]
    public async Task TestValidateAsync_WithInvalidRuleName_ShouldHaveValidationError(string ruleName)
    {
        // Arrange
        var deleteDeviationRequest = _fixture.Build<DeleteDeviationRequest>()
            .With(x => x.RuleName, ruleName).Create();

        // Act
        var actual = await _sut.TestValidateAsync(deleteDeviationRequest);

        // Assert
        actual.ShouldHaveValidationErrorFor(x => x.RuleName);
    }

    [Fact]
    public async Task TestValidateAsync_WithInvalidForeignProjectId_ShouldHaveValidationError()
    {
        // Arrange
        var deleteDeviationRequest = _fixture.Build<DeleteDeviationRequest>()
            .With(x => x.ForeignProjectId, Guid.Parse(InvalidGuid)).Create();

        // Act
        var actual = await _sut.TestValidateAsync(deleteDeviationRequest);

        // Assert
        actual.ShouldHaveValidationErrorFor(x => x.ForeignProjectId);
    }

    [Theory]
    [InlineData(EmptyString)]
    [InlineData("1234567")]
    [InlineData("AB1234567")]
    public async Task TestValidateAsync_WithInvalidCiIdentifier_ShouldHaveValidationError(string ciIdentifier)
    {
        // Arrange
        var deleteDeviationRequest = _fixture.Build<DeleteDeviationRequest>()
            .With(x => x.CiIdentifier, ciIdentifier).Create();

        // Act
        var actual = await _sut.TestValidateAsync(deleteDeviationRequest);

        // Assert
        actual.ShouldHaveValidationErrorFor(x => x.CiIdentifier);
    }

    [Fact]
    public async Task TestValidateAsync_WithValidProperties_ShouldNotHaveValidationError()
    {
        // Arrange
        var deleteDeviationRequest = _fixture.Build<DeleteDeviationRequest>()
            .With(x => x.CiIdentifier, ValidCiIdentifier)
            .With(x => x.ItemId, _fixture.Create<int>().ToString())
            .With(x => x.RuleName, RuleNames.NobodyCanDeleteBuilds).Create();

        // Act
        var actual = await _sut.TestValidateAsync(deleteDeviationRequest);

        // Assert
        actual.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task TestValidateAsync_WithDummyItemId_ShouldNotHaveValidationError()
    {
        // Arrange
        var deleteDeviationRequest = _fixture.Build<DeleteDeviationRequest>()
            .With(x => x.CiIdentifier, ValidCiIdentifier)
            .With(x => x.ItemId, "Dummy")
            .With(x => x.RuleName, RuleNames.NobodyCanDeleteBuilds).Create();

        // Act
        var actual = await _sut.TestValidateAsync(deleteDeviationRequest);

        // Assert
        actual.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task TestValidateAsync_WithNullForeignProjectId_ShouldNotHaveValidationError()
    {
        // Arrange
        var deleteDeviationRequest = _fixture.Build<DeleteDeviationRequest>()
            .Without(x => x.ForeignProjectId).Create();

        // Act
        var actual = await _sut.TestValidateAsync(deleteDeviationRequest);

        // Assert
        actual.ShouldNotHaveValidationErrorFor(x => x.ForeignProjectId);
    }
}