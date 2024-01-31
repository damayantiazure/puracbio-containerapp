using FluentValidation.TestHelper;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Requests.RequestValidation;
using Rabobank.Compliancy.Domain.Rules;

namespace Rabobank.Compliancy.Application.Tests.Requests.RequestValidation;

public class ReconcileRequestValidatorTests
{
    private readonly ReconcileRequestValidator _sut;
    private readonly IFixture _fixture = new Fixture();
    private const string EmptyString = "";
    private const string WhiteSpaceString = " ";
    private const string InvalidGuid = "00000000-0000-0000-0000-000000000000";
    public ReconcileRequestValidatorTests()
    {
        _sut = new ReconcileRequestValidator();
    }

    [Theory]
    [InlineData(EmptyString)]
    [InlineData(WhiteSpaceString)]
    [InlineData(null)]
    public async Task TestValidateAsync_WithNoNullOrEmptyOrganization_ShouldHaveValidationError(string organization)
    {
        // Arrange
        var reconcileRequest = _fixture.Build<ReconcileRequest>()
            .With(x => x.Organization, organization).Create();

        // Act
        var actual = await _sut.TestValidateAsync(reconcileRequest);

        // Assert
        actual.ShouldHaveValidationErrorFor(x => x.Organization);
    }

    [Theory]
    [InlineData(EmptyString)]
    [InlineData(WhiteSpaceString)]
    [InlineData(null)]
    [InlineData("test")]
    public async Task TestValidateAsync_WithNoNullOrEmptyRuleName_ShouldHaveValidationError(string ruleName)
    {
        // Arrange
        var reconcileRequest = _fixture.Build<ReconcileRequest>()
            .With(x => x.RuleName, ruleName).Create();

        // Act
        var actual = await _sut.TestValidateAsync(reconcileRequest);

        // Assert
        actual.ShouldHaveValidationErrorFor(x => x.RuleName);
    }

    [Theory]
    [InlineData(EmptyString)]
    [InlineData(WhiteSpaceString)]
    [InlineData("-1")]
    [InlineData(InvalidGuid)]
    [InlineData(null)]
    public async Task TestValidateAsync_WithNoNullOrEmptyItemId_ShouldHaveValidationError(string itemId)
    {
        // Arrange
        var reconcileRequest = _fixture.Build<ReconcileRequest>()
            .With(x => x.RuleName, RuleNames.NobodyCanDeleteBuilds)
            .With(x => x.ItemId, itemId).Create();

        // Act
        var actual = await _sut.TestValidateAsync(reconcileRequest);

        // Assert
        actual.ShouldHaveValidationErrorFor(x => x.ItemId);
    }

    [Theory]
    [InlineData(InvalidGuid)]
    public async Task TestValidateAsync_WithNoNullOrEmptyProjectId_ShouldHaveValidationError(Guid projectId)
    {
        // Arrange
        var reconcileRequest = _fixture.Build<ReconcileRequest>()
            .With(x => x.ProjectId, projectId).Create();

        // Act
        var actual = await _sut.TestValidateAsync(reconcileRequest);

        // Assert
        actual.ShouldHaveValidationErrorFor(x => x.ProjectId);
    }

    [Fact]
    public async Task TestValidateAsync_WithCorrectlyFilledProperties_ShouldNotHaveValidationError()
    {
        // Arrange
        var reconcileRequest = _fixture.Build<ReconcileRequest>()
            .With(x => x.RuleName, RuleNames.NobodyCanDeleteBuilds)
            .With(x => x.ItemId, _fixture.Create<Guid>().ToString()).Create();

        // Act
        var actual = await _sut.TestValidateAsync(reconcileRequest);

        // Assert
        actual.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(nameof(RuleNames.NobodyCanDeleteBuilds))]
    [InlineData("nobodycandeletebuilds")]
    public async Task TestValidateAsync_WithCorrectlyFilledRuleName_ShouldNotHaveValidationError(string ruleName)
    {
        // Arrange
        var reconcileRequest = _fixture.Build<ReconcileRequest>()
            .With(x => x.RuleName, ruleName).Create();

        // Act
        var actual = await _sut.TestValidateAsync(reconcileRequest);

        // Assert
        actual.ShouldNotHaveValidationErrorFor(x => x.RuleName);
    }

    [Fact]
    public async Task TestValidateAsync_WithGuidAsItemId_ShouldNotHaveValidationError()
    {
        // Arrange
        var reconcileRequest = _fixture.Build<ReconcileRequest>()
            .With(x => x.ItemId, _fixture.Create<Guid>().ToString()).Create();

        // Act
        var actual = await _sut.TestValidateAsync(reconcileRequest);

        // Assert
        actual.ShouldNotHaveValidationErrorFor(x => x.ItemId);
    }

    [Fact]
    public async Task TestValidateAsync_WithIntegerAsItemId_ShouldNotHaveValidationError()
    {
        // Arrange
        var reconcileRequest = _fixture.Build<ReconcileRequest>()
            .With(x => x.ItemId, _fixture.Create<int>().ToString()).Create();

        // Act
        var actual = await _sut.TestValidateAsync(reconcileRequest);

        // Assert
        actual.ShouldNotHaveValidationErrorFor(x => x.ItemId);
    }
}