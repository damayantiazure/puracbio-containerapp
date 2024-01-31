using FluentValidation.TestHelper;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Requests.RequestValidation;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Rules;

namespace Rabobank.Compliancy.Application.Tests.Requests.RequestValidation;

public class RegisterDeviationRequestValidatorTests
{
    private readonly IFixture _fixture = new Fixture();

    private readonly RegisterDeviationRequestValidator _sut;

    public RegisterDeviationRequestValidatorTests()
    {
        _sut = new RegisterDeviationRequestValidator();
    }

    [Fact]
    public async Task TestValidateAsync_WithEmptyProperties_ShouldHaveValidationError()
    {
        // Arrange
        var registerDeviationRequest = new RegisterDeviationRequest();

        // Act
        var actual = await _sut.TestValidateAsync(registerDeviationRequest);

        // Assert
        actual.ShouldHaveValidationErrorFor(x => x.Organization);
        actual.ShouldHaveValidationErrorFor(x => x.ProjectId);
        actual.ShouldHaveValidationErrorFor(x => x.CiIdentifier);
        actual.ShouldHaveValidationErrorFor(x => x.RuleName);
        actual.ShouldHaveValidationErrorFor(x => x.ItemId);
        actual.ShouldHaveValidationErrorFor(x => x.Reason);
        actual.ShouldHaveValidationErrorFor(x => x.Comment);
    }

    [Fact]
    public async Task TestValidateAsync_WithRuleNotApplicableReason_ShouldHaveValidationErrorForReasonNotApplicable()
    {
        // Arrange
        var registerDeviationRequest = _fixture.Build<RegisterDeviationRequest>()
            .With(x => x.Reason, DeviationReason.RuleNotApplicable)
            .Without(x => x.ReasonNotApplicable)
            .Create();

        // Act
        var actual = await _sut.TestValidateAsync(registerDeviationRequest);

        // Assert
        actual.ShouldHaveValidationErrorFor(x => x.ReasonNotApplicable);
    }

    [Fact]
    public async Task TestValidateAsync_WithRuleNotApplicableReason_ShouldHaveValidationErrorForReasonOther()
    {
        // Arrange
        var registerDeviationRequest = _fixture.Build<RegisterDeviationRequest>()
            .With(x => x.Reason, DeviationReason.Other)
            .Without(x => x.ReasonOther)
            .Create();

        // Act
        var actual = await _sut.TestValidateAsync(registerDeviationRequest);

        // Assert
        actual.ShouldHaveValidationErrorFor(x => x.ReasonOther);
    }

    [Fact]
    public async Task TestValidateAsync_WithRuleNotApplicableReason_ShouldHaveValidationErrorForReasonNotApplicableOther()
    {
        // Arrange
        var registerDeviationRequest = _fixture.Build<RegisterDeviationRequest>()
            .With(x => x.ReasonNotApplicable, DeviationApplicabilityReason.Other)
            .Without(x => x.ReasonNotApplicableOther)
            .Create();

        // Act
        var actual = await _sut.TestValidateAsync(registerDeviationRequest);

        // Assert
        actual.ShouldHaveValidationErrorFor(x => x.ReasonNotApplicableOther);
    }

    [Fact]
    public async Task TestValidateAsync_WithValidProperties_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var registerDeviationRequest = _fixture.Build<RegisterDeviationRequest>()
            .With(x => x.ItemId, "123")
            .With(x => x.RuleName, RuleNames.NobodyCanDeleteTheRepository)
            .Create();

        // Act
        var actual = await _sut.TestValidateAsync(registerDeviationRequest);

        // Assert
        actual.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("e1d3e759-de1e-4695-9026-9e70125759d9")]
    [InlineData("123")]
    [InlineData("Dummy")]
    public async Task TestValidateAsync_WithValidItemId_ShouldNotHaveValidationErrorForItemId(string itemId)
    {
        // Arrange
        var registerDeviationRequest = _fixture.Build<RegisterDeviationRequest>()
            .With(x => x.ItemId, itemId)
            .Create();

        // Act
        var actual = await _sut.TestValidateAsync(registerDeviationRequest);

        // Assert
        actual.ShouldNotHaveValidationErrorFor(x => x.ItemId);
    }

    [Theory]
    [InlineData(RuleNames.NobodyCanDeleteTheRepository)]
    public async Task TestValidateAsync_WithValidRulename_ShouldNotHaveValidationErrorForRuleName(string ruleName)
    {
        // Arrange
        var registerDeviationRequest = _fixture.Build<RegisterDeviationRequest>()
            .With(x => x.RuleName, ruleName)
            .Create();

        // Act
        var actual = await _sut.TestValidateAsync(registerDeviationRequest);

        // Assert

        actual.ShouldNotHaveValidationErrorFor(x => x.RuleName);
    }
}